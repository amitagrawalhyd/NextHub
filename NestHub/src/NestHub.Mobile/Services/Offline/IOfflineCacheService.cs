namespace NestHub.Mobile.Services.Offline;

/// <summary>
/// Generic JSON cache-aside store backed by <see cref="FileSystem.AppDataDirectory"/>, used to
/// keep the last-known-good result of a high-traffic read (vendor search, broadcast feed) available
/// when a live fetch fails.
/// </summary>
public interface IOfflineCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value);
}
