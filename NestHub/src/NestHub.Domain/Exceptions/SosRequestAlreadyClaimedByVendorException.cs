using NestHub.Domain.Common;

namespace NestHub.Domain.Exceptions;

public sealed class SosRequestAlreadyClaimedByVendorException : DomainException
{
    public SosRequestAlreadyClaimedByVendorException(SosRequestId sosRequestId, VendorId vendorId)
        : base($"Vendor '{vendorId}' has already claimed SOS request '{sosRequestId}'.")
    {
    }
}
