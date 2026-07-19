using FluentAssertions;
using Moq;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Vendors.Queries.SearchVendors;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.UnitTests.Vendors;

public class SearchVendorsQueryHandlerTests
{
    private static Vendor CreateApprovedVendor(string businessName, double rating)
    {
        var vendor = Vendor.Register(UserId.New(), businessName, PhoneNumber.Create("9876543210"), null, OperatingHours.AlwaysOpen());
        vendor.Approve();
        vendor.RecalculateAverageRating(rating);
        return vendor;
    }

    [Fact]
    public async Task Handle_SortsInHouseTierBeforeHigherRatedOtherTierVendor()
    {
        var residentSocietyId = SocietyId.New();

        var inHouseVendor = CreateApprovedVendor("Sharma Plumbing Works", 3.0);
        var otherVendor = CreateApprovedVendor("BrightSpark Electricians", 5.0);

        var vendorRepository = new Mock<IVendorRepository>();
        vendorRepository.Setup(r => r.GetAllApprovedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vendor> { otherVendor, inHouseVendor }); // deliberately Other-tier-first pre-sort

        var coverageRepository = new Mock<IVendorSocietyCoverageRepository>();
        coverageRepository.Setup(r => r.GetAllForSocietyAsync(residentSocietyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<VendorSocietyCoverage> { VendorSocietyCoverage.Create(inHouseVendor.Id, residentSocietyId, AffiliationType.InHouse) });

        var aiService = new Mock<IAiService>();

        var handler = new SearchVendorsQueryHandler(vendorRepository.Object, aiService.Object, coverageRepository.Object);

        var result = await handler.Handle(new SearchVendorsQuery(null, null, residentSocietyId.Value), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items[0].BusinessName.Should().Be("Sharma Plumbing Works");
        result.Items[0].Tier.Should().Be("InHouse");
        result.Items[1].BusinessName.Should().Be("BrightSpark Electricians");
        result.Items[1].Tier.Should().Be("Other");
    }

    [Fact]
    public async Task Handle_PaginatesAfterTiering()
    {
        var vendors = Enumerable.Range(0, 5).Select(i => CreateApprovedVendor($"Vendor {i}", 4.0)).ToList();

        var vendorRepository = new Mock<IVendorRepository>();
        vendorRepository.Setup(r => r.GetAllApprovedAsync(It.IsAny<CancellationToken>())).ReturnsAsync(vendors);

        var coverageRepository = new Mock<IVendorSocietyCoverageRepository>();
        var aiService = new Mock<IAiService>();
        var handler = new SearchVendorsQueryHandler(vendorRepository.Object, aiService.Object, coverageRepository.Object);

        var result = await handler.Handle(new SearchVendorsQuery(null, null, null, PageNumber: 2, PageSize: 2), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.PageNumber.Should().Be(2);
    }
}
