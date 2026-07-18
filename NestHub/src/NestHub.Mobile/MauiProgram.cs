using Microsoft.Extensions.Logging;
using NestHub.Mobile.Services.Api;
using NestHub.Mobile.Services.Auth;
using NestHub.Mobile.Services.Native;
using NestHub.Mobile.Services.Realtime;
using NestHub.Mobile.ViewModels;
using NestHub.Mobile.ViewModels.Resident;
using NestHub.Mobile.ViewModels.Vendor;
using NestHub.Mobile.Views;
using NestHub.Mobile.Views.Resident;
using NestHub.Mobile.Views.Vendor;

namespace NestHub.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<AuthSession>();
        builder.Services.AddSingleton<SosHubClient>();

        builder.Services.AddHttpClient<ApiClient>(client =>
        {
            client.BaseAddress = new Uri(Services.AppConfig.ApiBaseAddress);
        });

        builder.Services.AddSingleton<IPhoneDialerService, PhoneDialerService>();
        builder.Services.AddSingleton<IWhatsAppService, WhatsAppService>();
        builder.Services.AddSingleton<INoObligationInterceptor, NoObligationInterceptor>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ResidentOnboardingViewModel>();
        builder.Services.AddTransient<VendorSearchViewModel>();
        builder.Services.AddTransient<VendorProfileViewModel>();
        builder.Services.AddTransient<SosViewModel>();
        builder.Services.AddTransient<VendorOnboardingViewModel>();
        builder.Services.AddTransient<VendorDashboardViewModel>();
        builder.Services.AddTransient<VendorSosLeadsViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<VendorSearchPage>();
        builder.Services.AddTransient<VendorProfilePage>();
        builder.Services.AddTransient<SosPage>();
        builder.Services.AddTransient<ResidentOnboardingPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<SosLeadsPage>();
        builder.Services.AddTransient<VendorOnboardingPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
