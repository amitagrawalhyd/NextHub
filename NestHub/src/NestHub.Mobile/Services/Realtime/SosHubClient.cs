using Microsoft.AspNetCore.SignalR.Client;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile.Services.Realtime;

public sealed record SosRequestCreatedMessage(Guid SosRequestId, Guid SocietyId, string Category, string Description);

public sealed record SosRequestClaimedMessage(Guid SosRequestId, Guid VendorId);

public sealed record VendorBroadcastCreatedMessage(Guid BroadcastId, Guid VendorId, string BusinessName, string Title, string Message);

/// <summary>
/// Real-time transport for the vendor "Priority SOS Push Listener", resident claim notifications,
/// and resident vendor-broadcast delivery.
/// </summary>
public sealed class SosHubClient : IAsyncDisposable
{
    private readonly AuthSession _authSession;
    private HubConnection? _connection;

    public event Action<SosRequestCreatedMessage>? SosRequestCreated;
    public event Action<SosRequestClaimedMessage>? SosRequestClaimed;
    public event Action<VendorBroadcastCreatedMessage>? VendorBroadcastCreated;

    public SosHubClient(AuthSession authSession) => _authSession = authSession;

    /// <summary>
    /// Idempotent — safe to call from every page's OnAppearing; only the first call actually
    /// opens the connection, subsequent calls are no-ops while it's already open/connecting.
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_connection is not null)
            return;

        _connection = new HubConnectionBuilder()
            .WithUrl($"{AppConfig.ApiBaseAddress.TrimEnd('/')}/hubs/sos", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_authSession.Token);
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<SosRequestCreatedMessage>("SosRequestCreated", message => SosRequestCreated?.Invoke(message));
        _connection.On<SosRequestClaimedMessage>("SosRequestClaimed", message => SosRequestClaimed?.Invoke(message));
        _connection.On<VendorBroadcastCreatedMessage>("VendorBroadcastCreated", message => VendorBroadcastCreated?.Invoke(message));

        await _connection.StartAsync();
    }

    public Task JoinSocietyCategoryGroupAsync(Guid societyId, string category) =>
        _connection?.InvokeAsync("JoinSocietyCategoryGroup", societyId, category) ?? Task.CompletedTask;

    public Task JoinResidentGroupAsync(Guid residentId) =>
        _connection?.InvokeAsync("JoinResidentGroup", residentId) ?? Task.CompletedTask;

    public Task JoinSocietyBroadcastGroupAsync(Guid societyId) =>
        _connection?.InvokeAsync("JoinSocietyBroadcastGroup", societyId) ?? Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
