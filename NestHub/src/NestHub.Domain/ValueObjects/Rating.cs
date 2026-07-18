using NestHub.Domain.Exceptions;

namespace NestHub.Domain.ValueObjects;

public readonly record struct Rating
{
    public int Value { get; }

    private Rating(int value) => Value = value;

    public static Rating Create(int value)
    {
        if (value is < 1 or > 5)
            throw new InvalidRatingException(value);

        return new Rating(value);
    }

    public static implicit operator int(Rating rating) => rating.Value;

    public override string ToString() => Value.ToString();
}
