using NestHub.Domain.Common;

namespace NestHub.Domain.Vendors;

public sealed class VendorBroadcast : Entity<VendorBroadcastId>
{
    public VendorId VendorId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public DateTime CreatedDateTimeUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsActive => !ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > DateTime.UtcNow;

    private VendorBroadcast()
    {
    }

    private VendorBroadcast(VendorBroadcastId id, VendorId vendorId, string title, string message, DateTime? expiresAtUtc)
    {
        Id = id;
        VendorId = vendorId;
        Title = title;
        Message = message;
        CreatedDateTimeUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
    }

    public static VendorBroadcast Create(VendorId vendorId, string title, string message, DateTime? expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required.", nameof(message));

        return new VendorBroadcast(VendorBroadcastId.New(), vendorId, title.Trim(), message.Trim(), expiresAtUtc);
    }
}
