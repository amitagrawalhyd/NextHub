namespace NestHub.Mobile.Services.Native;

public interface IWhatsAppService
{
    Task OpenChatAsync(string phoneNumber, string message);
}
