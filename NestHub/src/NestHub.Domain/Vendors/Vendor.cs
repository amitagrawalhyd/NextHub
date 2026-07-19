using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Exceptions;
using NestHub.Domain.ValueObjects;
using NetTopologySuite.Geometries;

namespace NestHub.Domain.Vendors;

public sealed class Vendor : AggregateRoot<VendorId>
{
    public const int FreeTierMaxServices = 3;

    private readonly List<Service> _services = new();

    public UserId UserId { get; private set; }
    public string BusinessName { get; private set; } = null!;
    public string? LogoUrl { get; private set; }
    public string? Bio { get; private set; }
    public PhoneNumber WhatsAppNumber { get; private set; } = null!;
    public OperatingHours OperatingHours { get; private set; } = null!;
    public SubscriptionTier SubscriptionTier { get; private set; }
    public TrustBadgeStatus TrustBadgeStatus { get; private set; }
    public double AverageRating { get; private set; }
    public bool IsApproved { get; private set; }
    public bool IsFeatured { get; private set; }
    public IReadOnlyCollection<Service> Services => _services.AsReadOnly();

    /// <summary>
    /// The EF-mapped, spatially-indexed persisted column. <see cref="GeoLocation"/> below is
    /// the business-facing façade every other layer reads/writes — see Society for the same pattern.
    /// </summary>
    public Point? Location { get; private set; }

    public GeoLocation? GeoLocation => Location is null ? null : ValueObjects.GeoLocation.FromPoint(Location);

    private Vendor()
    {
    }

    private Vendor(VendorId id, UserId userId, string businessName, PhoneNumber whatsAppNumber, string? bio, OperatingHours operatingHours, GeoLocation? location)
    {
        Id = id;
        UserId = userId;
        BusinessName = businessName;
        WhatsAppNumber = whatsAppNumber;
        Bio = bio;
        OperatingHours = operatingHours;
        SubscriptionTier = SubscriptionTier.Free;
        TrustBadgeStatus = TrustBadgeStatus.None;
        AverageRating = 0;
        IsApproved = false;
        Location = location?.ToPoint();
    }

    public static Vendor Register(UserId userId, string businessName, PhoneNumber whatsAppNumber, string? bio, OperatingHours operatingHours, GeoLocation? location = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));

        var vendor = new Vendor(VendorId.New(), userId, businessName.Trim(), whatsAppNumber, bio?.Trim(), operatingHours, location);
        vendor.RaiseDomainEvent(new VendorRegisteredDomainEvent(vendor.Id, userId));
        if (location is not null)
            vendor.RaiseDomainEvent(new VendorLocationChangedDomainEvent(vendor.Id));
        return vendor;
    }

    public void Approve()
    {
        if (IsApproved) return;
        IsApproved = true;
        RaiseDomainEvent(new VendorApprovedDomainEvent(Id));
    }

    public void UpdateProfile(string businessName, string? bio, string? logoUrl, PhoneNumber whatsAppNumber, OperatingHours operatingHours, GeoLocation? location)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));

        BusinessName = businessName.Trim();
        Bio = bio?.Trim();
        LogoUrl = logoUrl;
        WhatsAppNumber = whatsAppNumber;
        OperatingHours = operatingHours;

        var newLocation = location?.ToPoint();
        var locationChanged = (Location is null) != (newLocation is null)
            || (Location is not null && newLocation is not null && !Location.Equals(newLocation));
        if (locationChanged)
        {
            Location = newLocation;
            RaiseDomainEvent(new VendorLocationChangedDomainEvent(Id));
        }
    }

    public Service AddService(string title, string description, PricingInfo pricing, string category)
    {
        if (!IsApproved)
            throw new VendorNotApprovedException(Id);
        if (SubscriptionTier == SubscriptionTier.Free && _services.Count >= FreeTierMaxServices)
            throw new FreeTierServiceLimitExceededException(Id);

        var service = Service.Create(Id, title, description, pricing, category);
        _services.Add(service);
        RaiseDomainEvent(new ServiceAddedDomainEvent(Id, service.Id));
        return service;
    }

    public void RemoveService(ServiceId serviceId)
    {
        var service = _services.FirstOrDefault(s => s.Id == serviceId);
        if (service is not null)
            _services.Remove(service);
    }

    public void UpgradeToPremium()
    {
        if (SubscriptionTier == SubscriptionTier.Premium) return;
        SubscriptionTier = SubscriptionTier.Premium;
        RaiseDomainEvent(new VendorUpgradedToPremiumDomainEvent(Id));
    }

    public void DowngradeToFree() => SubscriptionTier = SubscriptionTier.Free;

    public void AwardTrustBadge(TrustBadgeStatus badge)
    {
        TrustBadgeStatus = badge;
        RaiseDomainEvent(new TrustBadgeAwardedDomainEvent(Id, badge));
    }

    public void RecalculateAverageRating(double newAverage)
    {
        if (newAverage is < 0 or > 5)
            throw new ArgumentOutOfRangeException(nameof(newAverage), newAverage, "Average rating must be between 0 and 5.");
        AverageRating = newAverage;
    }

    public void MarkFeatured() => IsFeatured = true;

    public void UnmarkFeatured() => IsFeatured = false;
}
