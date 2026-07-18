namespace NestHub.Mobile.Services.Native;

public sealed class PhoneDialerService : IPhoneDialerService
{
    public bool IsSupported => PhoneDialer.Default.IsSupported;

    public void Call(string phoneNumber) => PhoneDialer.Default.Open(phoneNumber);
}
