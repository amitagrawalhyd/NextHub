using System.Net;

namespace NestHub.Mobile.Services.Native;

public sealed class WhatsAppService : IWhatsAppService
{
    private const string DefaultCountryCode = "91"; // Vendor WhatsApp numbers are entered as plain Indian mobile numbers with no country code.

    public Task OpenChatAsync(string phoneNumber, string message)
    {
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // wa.me requires the full international number (country code + subscriber number, no
        // leading zeros or symbols) — a bare 10-digit local number is rejected by WhatsApp as
        // "missing a country code". Vendor numbers are collected as plain 10-digit numbers, so
        // normalize them to E.164-style digits before building the link.
        if (digitsOnly.Length == 11 && digitsOnly.StartsWith('0'))
            digitsOnly = digitsOnly[1..];

        if (digitsOnly.Length == 10)
            digitsOnly = DefaultCountryCode + digitsOnly;

        var uri = new Uri($"https://wa.me/{digitsOnly}?text={WebUtility.UrlEncode(message)}");
        return Launcher.Default.OpenAsync(uri);
    }
}
