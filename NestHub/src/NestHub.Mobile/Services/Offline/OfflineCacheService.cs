using System.Text.Json;

namespace NestHub.Mobile.Services.Offline;

public sealed class OfflineCacheService : IOfflineCacheService
{
    public async Task<T?> GetAsync<T>(string key)
    {
        var path = GetPath(key);
        if (!File.Exists(path))
            return default;

        try
        {
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var path = GetPath(key);
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, value);
    }

    private static string GetPath(string key) => Path.Combine(FileSystem.AppDataDirectory, $"cache_{key}.json");
}
