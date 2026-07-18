namespace NestHub.Mobile.Services.Native;

public interface IPhoneDialerService
{
    bool IsSupported { get; }

    void Call(string phoneNumber);
}
