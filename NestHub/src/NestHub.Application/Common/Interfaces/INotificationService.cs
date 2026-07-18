using NestHub.Domain.Common;

namespace NestHub.Application.Common.Interfaces;

public interface INotificationService
{
    Task BroadcastSosRequestAsync(SosRequestId sosRequestId, SocietyId societyId, string category, string description, CancellationToken cancellationToken = default);

    Task NotifySosRequestClaimedAsync(SosRequestId sosRequestId, ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default);

    Task BroadcastVendorUpdateAsync(Guid broadcastId, VendorId vendorId, string businessName, string title, string message, IEnumerable<SocietyId> societyIds, CancellationToken cancellationToken = default);
}
