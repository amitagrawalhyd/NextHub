using NestHub.Domain.Common;

namespace NestHub.Application.Common.Interfaces;

public interface INotificationService
{
    Task BroadcastSosRequestAsync(SosRequestId sosRequestId, SocietyId societyId, string category, string description, CancellationToken cancellationToken = default);

    Task NotifySosRequestClaimedAsync(SosRequestId sosRequestId, ResidentId residentId, VendorId vendorId, CancellationToken cancellationToken = default);
}
