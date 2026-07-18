using FluentAssertions;
using Moq;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Reviews.Commands.SubmitReview;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Reviews;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.UnitTests.Reviews;

public class SubmitReviewCommandHandlerTests
{
    private static Vendor CreateVendor() =>
        Vendor.Register(UserId.New(), "Sharma Plumbing Works", PhoneNumber.Create("9876543210"), null, OperatingHours.AlwaysOpen());

    [Fact]
    public async Task Handle_WithComment_AppliesSentimentScoreFromAiService()
    {
        var vendor = CreateVendor();
        var vendorRepository = new Mock<IVendorRepository>();
        vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);

        var reviewRepository = new Mock<IReviewRepository>();
        reviewRepository.Setup(r => r.GetByVendorAsync(vendor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Review>());

        var aiService = new Mock<IAiService>();
        aiService.Setup(a => a.ScoreSentiment("Excellent service!")).Returns(0.9);

        var handler = new SubmitReviewCommandHandler(reviewRepository.Object, vendorRepository.Object, Mock.Of<IUnitOfWork>(), aiService.Object);

        var command = new SubmitReviewCommand(Guid.NewGuid(), vendor.Id.Value, Guid.NewGuid(), 5, "Excellent service!");

        var result = await handler.Handle(command, CancellationToken.None);

        result.SentimentScore.Should().Be(0.9);
        vendor.AverageRating.Should().Be(5);
    }

    [Fact]
    public async Task Handle_RecalculatesVendorAverageRatingAcrossExistingReviews()
    {
        var vendor = CreateVendor();
        var vendorRepository = new Mock<IVendorRepository>();
        vendorRepository.Setup(r => r.GetByIdAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(vendor);

        var existingReviews = new List<Review>
        {
            Review.Submit(ResidentId.New(), vendor.Id, SocietyId.New(), Rating.Create(3), null),
            Review.Submit(ResidentId.New(), vendor.Id, SocietyId.New(), Rating.Create(5), null)
        };

        var reviewRepository = new Mock<IReviewRepository>();
        reviewRepository.Setup(r => r.GetByVendorAsync(vendor.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existingReviews);

        var handler = new SubmitReviewCommandHandler(
            reviewRepository.Object,
            vendorRepository.Object,
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IAiService>());

        var command = new SubmitReviewCommand(Guid.NewGuid(), vendor.Id.Value, Guid.NewGuid(), 4, null);

        await handler.Handle(command, CancellationToken.None);

        vendor.AverageRating.Should().Be(4);
    }
}
