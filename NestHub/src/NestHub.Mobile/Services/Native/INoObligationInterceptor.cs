namespace NestHub.Mobile.Services.Native;

/// <summary>
/// Gates direct-contact actions (call/WhatsApp) behind a one-time "No Obligation" disclaimer,
/// as required by NestHub's ToS: the platform does not vet or guarantee vendor service quality.
/// </summary>
public interface INoObligationInterceptor
{
    Task InterceptAsync(Func<Task> action);
}
