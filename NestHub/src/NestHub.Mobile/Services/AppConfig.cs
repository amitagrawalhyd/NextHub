namespace NestHub.Mobile.Services;

public static class AppConfig
{
    /// <summary>
    /// 10.0.2.2 is the Android emulator's alias for the host machine's localhost. For a physical
    /// device or iOS simulator during development, replace with your machine's LAN IP; for
    /// production this should point at the deployed NestHub.API base URL (see DEPLOYMENT.md).
    /// </summary>
    public static string ApiBaseAddress { get; } =
#if ANDROID
        "http://10.0.2.2:5126/";
#else
        "http://localhost:5126/";
#endif
}
