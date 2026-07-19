using FluentAssertions;
using NestHub.Domain.Common;
using NestHub.Domain.Exceptions;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Domain.UnitTests.Vendors;

public class VendorTests
{
    private static Vendor CreateApprovedVendor()
    {
        var vendor = Vendor.Register(
            UserId.New(),
            "Sharma Plumbing Works",
            PhoneNumber.Create("9876543210"),
            "Reliable plumbing services.",
            OperatingHours.AlwaysOpen());

        vendor.Approve();
        return vendor;
    }

    [Fact]
    public void Register_RaisesVendorRegisteredDomainEvent()
    {
        var vendor = Vendor.Register(
            UserId.New(),
            "Sharma Plumbing Works",
            PhoneNumber.Create("9876543210"),
            null,
            OperatingHours.AlwaysOpen());

        vendor.DomainEvents.Should().ContainSingle(e => e is VendorRegisteredDomainEvent);
        vendor.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void AddService_WhenNotApproved_ThrowsVendorNotApprovedException()
    {
        var vendor = Vendor.Register(
            UserId.New(),
            "Sharma Plumbing Works",
            PhoneNumber.Create("9876543210"),
            null,
            OperatingHours.AlwaysOpen());

        var act = () => vendor.AddService("Pipe Repair", "Fix leaking pipes.", PricingInfo.ContactForQuote(), "Home Maintenance");

        act.Should().Throw<VendorNotApprovedException>();
    }

    [Fact]
    public void AddService_OnFreeTierBeyondLimit_ThrowsFreeTierServiceLimitExceededException()
    {
        var vendor = CreateApprovedVendor();
        for (var i = 0; i < Vendor.FreeTierMaxServices; i++)
        {
            vendor.AddService($"Service {i}", "Description", PricingInfo.ContactForQuote(), "Home Maintenance");
        }

        var act = () => vendor.AddService("One too many", "Description", PricingInfo.ContactForQuote(), "Home Maintenance");

        act.Should().Throw<FreeTierServiceLimitExceededException>();
    }

    [Fact]
    public void AddService_AfterUpgradingToPremium_IsNotLimited()
    {
        var vendor = CreateApprovedVendor();
        for (var i = 0; i < Vendor.FreeTierMaxServices; i++)
        {
            vendor.AddService($"Service {i}", "Description", PricingInfo.ContactForQuote(), "Home Maintenance");
        }

        vendor.UpgradeToPremium();
        var act = () => vendor.AddService("Now allowed", "Description", PricingInfo.ContactForQuote(), "Home Maintenance");

        act.Should().NotThrow();
        vendor.Services.Should().HaveCount(Vendor.FreeTierMaxServices + 1);
    }

    [Fact]
    public void AwardTrustBadge_RaisesTrustBadgeAwardedDomainEvent()
    {
        var vendor = CreateApprovedVendor();

        vendor.AwardTrustBadge(Enums.TrustBadgeStatus.IdVerified);

        vendor.TrustBadgeStatus.Should().Be(Enums.TrustBadgeStatus.IdVerified);
        vendor.DomainEvents.Should().Contain(e => e is TrustBadgeAwardedDomainEvent);
    }

    [Fact]
    public void UpdateProfile_UpdatesBusinessNameBioLogoAndWhatsAppNumber()
    {
        var vendor = CreateApprovedVendor();
        var newWhatsAppNumber = PhoneNumber.Create("9123456780");

        vendor.UpdateProfile("Sharma Plumbing & Electrical", "Now also doing electrical work.", "https://example.com/logo.png", newWhatsAppNumber, vendor.OperatingHours, null);

        vendor.BusinessName.Should().Be("Sharma Plumbing & Electrical");
        vendor.Bio.Should().Be("Now also doing electrical work.");
        vendor.LogoUrl.Should().Be("https://example.com/logo.png");
        vendor.WhatsAppNumber.Should().Be(newWhatsAppNumber);
    }

    [Fact]
    public void UpdateProfile_WithBlankBusinessName_ThrowsArgumentException()
    {
        var vendor = CreateApprovedVendor();

        var act = () => vendor.UpdateProfile(" ", null, null, vendor.WhatsAppNumber, vendor.OperatingHours, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithNewLocation_SetsGeoLocationAndRaisesVendorLocationChangedDomainEvent()
    {
        var vendor = CreateApprovedVendor();
        var location = GeoLocation.Create(17.4448, 78.3498);

        vendor.UpdateProfile(vendor.BusinessName, vendor.Bio, vendor.LogoUrl, vendor.WhatsAppNumber, vendor.OperatingHours, location);

        vendor.GeoLocation.Should().NotBeNull();
        vendor.GeoLocation!.Latitude.Should().Be(17.4448);
        vendor.GeoLocation.Longitude.Should().Be(78.3498);
        vendor.DomainEvents.Should().ContainSingle(e => e is VendorLocationChangedDomainEvent);
    }

    [Fact]
    public void UpdateProfile_CalledTwiceWithSameLocation_RaisesLocationChangedEventOnlyOnce()
    {
        var vendor = CreateApprovedVendor();
        var location = GeoLocation.Create(17.4448, 78.3498);

        vendor.UpdateProfile(vendor.BusinessName, vendor.Bio, vendor.LogoUrl, vendor.WhatsAppNumber, vendor.OperatingHours, location);
        vendor.ClearDomainEvents();

        // Same coordinates again, only the bio changes — should not raise a second location event.
        vendor.UpdateProfile(vendor.BusinessName, "Updated bio only.", vendor.LogoUrl, vendor.WhatsAppNumber, vendor.OperatingHours, location);

        vendor.DomainEvents.Should().NotContain(e => e is VendorLocationChangedDomainEvent);
    }

    [Fact]
    public void UpdateProfile_WithoutLocationChange_DoesNotRaiseVendorLocationChangedDomainEvent()
    {
        var vendor = CreateApprovedVendor();

        vendor.UpdateProfile("Sharma Plumbing & Electrical", vendor.Bio, vendor.LogoUrl, vendor.WhatsAppNumber, vendor.OperatingHours, null);

        vendor.DomainEvents.Should().NotContain(e => e is VendorLocationChangedDomainEvent);
    }
}
