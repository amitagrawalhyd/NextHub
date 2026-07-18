using NestHub.Domain.Common;

namespace NestHub.Domain.Exceptions;

public sealed class FreeTierServiceLimitExceededException : DomainException
{
    public FreeTierServiceLimitExceededException(VendorId vendorId)
        : base($"Vendor '{vendorId}' is on the Free tier and has reached the maximum number of listed services. Upgrade to Premium to add more.")
    {
    }
}
