namespace NestHub.Mobile.Services;

public static class AppConfig
{
    /// <summary>
    /// Points at the deployed NestHub.API (Azure App Service, Free tier — see DEPLOYMENT.md).
    /// For local backend development, swap this for "http://10.0.2.2:5126/" (Android emulator)
    /// or "http://localhost:5126/" (other targets) to hit a locally running API instead.
    /// </summary>
    public static string ApiBaseAddress { get; } = "https://nesthub-api-amitagrawal.azurewebsites.net/";
}
