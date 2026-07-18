using NestHub.Mobile.Views;
using NestHub.Mobile.Views.Resident;
using NestHub.Mobile.Views.Vendor;

namespace NestHub.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("Register", typeof(RegisterPage));
        Routing.RegisterRoute("VendorProfile", typeof(VendorProfilePage));
        Routing.RegisterRoute("ResidentOnboarding", typeof(ResidentOnboardingPage));
        Routing.RegisterRoute("VendorOnboarding", typeof(VendorOnboardingPage));
    }
}
