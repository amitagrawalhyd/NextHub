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
}
