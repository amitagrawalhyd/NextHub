using NestHub.Application.Common.Interfaces;

namespace NestHub.Infrastructure.Services;

/// <summary>
/// Stores files on local disk under wwwroot/uploads. Suitable for single-instance deployments;
/// swap for a blob-storage-backed implementation behind the same <see cref="IFileStorageService"/>
/// interface for multi-instance/cloud deployments.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _publicBasePath;

    public LocalFileStorageService(string rootPath, string publicBasePath = "/uploads")
    {
        _rootPath = rootPath;
        _publicBasePath = publicBasePath;
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(_rootPath, safeFileName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"{_publicBasePath}/{safeFileName}";
    }
}
