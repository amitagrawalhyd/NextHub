using FluentAssertions;
using NestHub.Domain.Common;
using NestHub.Domain.Reviews;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.UnitTests.Reviews;

public class ReviewTests
{
    [Fact]
    public void Submit_RaisesReviewSubmittedDomainEvent()
    {
        var review = Review.Submit(ResidentId.New(), VendorId.New(), SocietyId.New(), Rating.Create(5), "Excellent service!");

        review.DomainEvents.Should().ContainSingle(e => e is ReviewSubmittedDomainEvent);
        review.IsFlagged.Should().BeFalse();
    }

    [Fact]
    public void ApplySentimentScore_OutsideRange_Throws()
    {
        var review = Review.Submit(ResidentId.New(), VendorId.New(), SocietyId.New(), Rating.Create(4), "Good");

        var act = () => review.ApplySentimentScore(1.5);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Flag_MarksReviewAsFlaggedAndRaisesEvent()
    {
        var review = Review.Submit(ResidentId.New(), VendorId.New(), SocietyId.New(), Rating.Create(1), "Bad experience");

        review.Flag();

        review.IsFlagged.Should().BeTrue();
        review.DomainEvents.Should().Contain(e => e is ReviewFlaggedDomainEvent);
    }
}
