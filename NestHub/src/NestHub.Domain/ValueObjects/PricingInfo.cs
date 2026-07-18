using NestHub.Domain.Enums;

namespace NestHub.Domain.ValueObjects;

public sealed record PricingInfo
{
    public PricingType Type { get; }
    public decimal? Amount { get; }
    public string Currency { get; }

    private PricingInfo(PricingType type, decimal? amount, string currency)
    {
        Type = type;
        Amount = amount;
        Currency = currency;
    }

    public static PricingInfo ContactForQuote() => new(PricingType.ContactForQuote, null, "INR");

    public static PricingInfo Fixed(decimal amount, string currency = "INR") =>
        new(PricingType.Fixed, RequirePositive(amount), currency);

    public static PricingInfo Hourly(decimal amount, string currency = "INR") =>
        new(PricingType.Hourly, RequirePositive(amount), currency);

    public static PricingInfo StartingFrom(decimal amount, string currency = "INR") =>
        new(PricingType.StartingFrom, RequirePositive(amount), currency);

    private static decimal RequirePositive(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be greater than zero.");
        return amount;
    }

    public override string ToString() => Type switch
    {
        PricingType.ContactForQuote => "Contact for Quote",
        PricingType.Hourly => $"{Currency} {Amount}/hour",
        PricingType.StartingFrom => $"Starting from {Currency} {Amount}",
        PricingType.Fixed => $"{Currency} {Amount}",
        _ => throw new ArgumentOutOfRangeException()
    };
}
