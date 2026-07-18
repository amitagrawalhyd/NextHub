using NestHub.Domain.Common;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Vendors;

public sealed class Service : Entity<ServiceId>
{
    public VendorId VendorId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public PricingInfo Pricing { get; private set; } = null!;
    public string Category { get; private set; } = null!;

    private Service()
    {
    }

    private Service(ServiceId id, VendorId vendorId, string title, string description, PricingInfo pricing, string category)
    {
        Id = id;
        VendorId = vendorId;
        Title = title;
        Description = description;
        Pricing = pricing;
        Category = category;
    }

    internal static Service Create(VendorId vendorId, string title, string description, PricingInfo pricing, string category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Service title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Service category is required.", nameof(category));

        return new Service(ServiceId.New(), vendorId, title.Trim(), description?.Trim() ?? string.Empty, pricing, category.Trim());
    }

    public void UpdateDetails(string title, string description, PricingInfo pricing, string category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Service title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Service category is required.", nameof(category));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Pricing = pricing;
        Category = category.Trim();
    }
}
