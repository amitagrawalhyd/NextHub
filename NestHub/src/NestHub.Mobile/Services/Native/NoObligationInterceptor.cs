namespace NestHub.Mobile.Services.Native;

public sealed class NoObligationInterceptor : INoObligationInterceptor
{
    private const string AcknowledgedPreferenceKey = "NoObligationDisclaimerAcknowledged";

    public async Task InterceptAsync(Func<Task> action)
    {
        var alreadyAcknowledged = Preferences.Default.Get(AcknowledgedPreferenceKey, false);

        if (!alreadyAcknowledged)
        {
            var page = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlert(
                    "Contacting the vendor directly",
                    "You are now contacting the vendor directly. NestHub is a discovery platform only and is not responsible for the service provided or any payment.",
                    "Continue");
            }

            Preferences.Default.Set(AcknowledgedPreferenceKey, true);
        }

        await action();
    }
}
