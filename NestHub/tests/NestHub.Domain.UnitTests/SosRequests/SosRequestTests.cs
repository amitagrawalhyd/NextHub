using FluentAssertions;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Exceptions;
using NestHub.Domain.SosRequests;

namespace NestHub.Domain.UnitTests.SosRequests;

public class SosRequestTests
{
    [Fact]
    public void RaiseNew_CreatesRequestInOpenStatus()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe in kitchen!");

        request.Status.Should().Be(SosStatus.Open);
        request.DomainEvents.Should().ContainSingle(e => e is SosRequestCreatedDomainEvent);
    }

    [Fact]
    public void ClaimBy_FirstVendor_MovesStatusToClaimed()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");
        var vendorId = VendorId.New();

        request.ClaimBy(vendorId);

        request.Status.Should().Be(SosStatus.Claimed);
        request.Claims.Should().ContainSingle(c => c.VendorId == vendorId);
    }

    [Fact]
    public void ClaimBy_SameVendorTwice_ThrowsSosRequestAlreadyClaimedByVendorException()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");
        var vendorId = VendorId.New();
        request.ClaimBy(vendorId);

        var act = () => request.ClaimBy(vendorId);

        act.Should().Throw<SosRequestAlreadyClaimedByVendorException>();
    }

    [Fact]
    public void ClaimBy_MultipleDifferentVendors_AllClaimsAreRecorded()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");

        request.ClaimBy(VendorId.New());
        request.ClaimBy(VendorId.New());

        request.Claims.Should().HaveCount(2);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ThrowsSosRequestClosedException()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");
        request.Close();

        var act = () => request.Close();

        act.Should().Throw<SosRequestClosedException>();
    }

    [Fact]
    public void ClaimBy_WhenClosed_ThrowsSosRequestClosedException()
    {
        var request = SosRequest.RaiseNew(ResidentId.New(), SocietyId.New(), "Plumbing", "Leaking pipe!");
        request.Close();

        var act = () => request.ClaimBy(VendorId.New());

        act.Should().Throw<SosRequestClosedException>();
    }
}
