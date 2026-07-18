using NestHub.Mobile.Resources.Strings;
using NestHub.Mobile.Services.Auth;

namespace NestHub.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly AuthSession _authSession;

    public App(AuthSession authSession)
    {
        LocalizationResourceManager.Instance.RestoreSavedCulture();

        InitializeComponent();
        _authSession = authSession;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Created += async (_, _) => await RestoreSessionAsync();
        return window;
    }

    private async Task RestoreSessionAsync()
    {
        await _authSession.RestoreAsync();

        if (!_authSession.IsAuthenticated)
            return;

        await Shell.Current.GoToAsync(_authSession.UserType == "Vendor" ? "//VendorShell" : "//ResidentShell");
    }
}
