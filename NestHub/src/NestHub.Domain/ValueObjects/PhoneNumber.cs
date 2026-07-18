using System.Text.RegularExpressions;

namespace NestHub.Domain.ValueObjects;

public sealed partial record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number is required.", nameof(value));

        var trimmed = value.Trim();
        if (!PhoneNumberRegex().IsMatch(trimmed))
            throw new ArgumentException("Phone number must be a valid number with an optional country code.", nameof(value));

        return new PhoneNumber(trimmed);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^\+?[1-9]\d{9,14}$")]
    private static partial Regex PhoneNumberRegex();
}
