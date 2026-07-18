using NestHub.Domain.Common;

namespace NestHub.Domain.Exceptions;

public sealed class VendorNotApprovedException : DomainException
{
    public VendorNotApprovedException(VendorId vendorId)
        : base($"Vendor '{vendorId}' has not been approved and cannot list services yet.")
    {
    }
}
