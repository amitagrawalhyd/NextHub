using NestHub.Domain.Common;

namespace NestHub.Domain.Exceptions;

public sealed class InvalidRatingException : DomainException
{
    public InvalidRatingException(int value)
        : base($"Rating must be between 1 and 5. Received: {value}.")
    {
    }
}
