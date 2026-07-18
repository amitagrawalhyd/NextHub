using Microsoft.AspNetCore.SignalR;

namespace NestHub.Infrastructure.Notifications;

/// <summary>
/// Real-time transport for the "SOS Urgent Request" broadcast and claim-notification flow.
/// Clients join a group per society+category (vendors) or per resident (residents) after connecting.
/// </summary>
public sealed class SosHub : Hub
{
    public Task JoinSocietyCategoryGroup(Guid societyId, string category) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.SocietyCategory(societyId, category));

    public Task JoinResidentGroup(Guid residentId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupNames.Resident(residentId));
}

public static class GroupNames
{
    public static string SocietyCategory(Guid societyId, string category) => $"society:{societyId}:category:{category.ToLowerInvariant()}";

    public static string Resident(Guid residentId) => $"resident:{residentId}";
}
