using NestHub.Domain.Common;

namespace NestHub.Domain.Exceptions;

public sealed class SosRequestClosedException : DomainException
{
    public SosRequestClosedException(SosRequestId sosRequestId)
        : base($"SOS request '{sosRequestId}' is already closed.")
    {
    }
}
