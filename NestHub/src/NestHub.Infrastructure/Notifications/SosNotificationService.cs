using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Common;

namespace NestHub.Infrastructure.Notifications;

public sealed class SosNotificationService : INotificationService
{
    private readonly IHubContext<SosHub> _hubContext;
    private readonly ILogger<SosNotificationService> _logger;

    public SosNotificationService(IHubContext<SosHub> hubContext, ILogger<SosNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task BroadcastSosRequestAsync(SosRequestId sosRequestId, SocietyId societyId, string category, string description, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Broadcasting SOS request {SosRequestId} for category {Category} in society {SocietyId}", sosRequestId, category, societyId);

        await _hubContext.Clients
            .Group(GroupNames.SocietyCategory(societyId.Value, category))
            .SendAsync("SosRequestCreated", new { sosRequestId = sosRequestId.Value, societyId = societyId.Value, category, description }, cancellationToken);
    }

    public async Task NotifySosRequestClaimedAsync(SosRequestId sosRequestId, ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SOS request {SosRequestId} claimed by vendor {VendorId}", sosRequestId, vendorId);

        await _hubContext.Clients
            .Group(GroupNames.Resident(residentId.Value))
            .SendAsync("SosRequestClaimed", new { sosRequestId = sosRequestId.Value, vendorId = vendorId.Value }, cancellationToken);
    }
}
