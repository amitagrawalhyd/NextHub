using System.Net;

namespace NestHub.Mobile.Services.Native;

public sealed class WhatsAppService : IWhatsAppService
{
    public Task OpenChatAsync(string phoneNumber, string message)
    {
        var digitsOnly = new string(phoneNumber.Where(char.IsDigit).ToArray());
        var uri = new Uri($"https://wa.me/{digitsOnly}?text={WebUtility.UrlEncode(message)}");
        return Launcher.Default.OpenAsync(uri);
    }
}
