using System.Text.RegularExpressions;

namespace NestHub.Domain.ValueObjects;

public sealed partial record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email is required.", nameof(value));

        var trimmed = value.Trim();
        if (!EmailRegex().IsMatch(trimmed))
            throw new ArgumentException("Email format is invalid.", nameof(value));

        return new Email(trimmed);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
