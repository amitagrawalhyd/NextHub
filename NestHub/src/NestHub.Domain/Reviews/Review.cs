using NestHub.Domain.Common;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Reviews;

public sealed class Review : AggregateRoot<ReviewId>
{
    public ResidentId ResidentId { get; private set; }
    public VendorId VendorId { get; private set; }
    public SocietyId SocietyId { get; private set; }
    public Rating Rating { get; private set; }
    public string? Comment { get; private set; }
    public double? SentimentScore { get; private set; }
    public bool IsFlagged { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private Review()
    {
    }

    private Review(ReviewId id, ResidentId residentId, VendorId vendorId, SocietyId societyId, Rating rating, string? comment)
    {
        Id = id;
        ResidentId = residentId;
        VendorId = vendorId;
        SocietyId = societyId;
        Rating = rating;
        Comment = comment;
        IsFlagged = false;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static Review Submit(ResidentId residentId, VendorId vendorId, SocietyId societyId, Rating rating, string? comment)
    {
        var review = new Review(ReviewId.New(), residentId, vendorId, societyId, rating, comment?.Trim());
        review.RaiseDomainEvent(new ReviewSubmittedDomainEvent(review.Id, vendorId, societyId, rating));
        return review;
    }

    public void ApplySentimentScore(double score)
    {
        if (score is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(score), score, "Sentiment score must be between -1 and 1.");
        SentimentScore = score;
    }

    public void Flag()
    {
        if (IsFlagged) return;
        IsFlagged = true;
        RaiseDomainEvent(new ReviewFlaggedDomainEvent(Id));
    }

    public void Unflag() => IsFlagged = false;

    public void MarkRemoved() => RaiseDomainEvent(new ReviewRemovedDomainEvent(Id, VendorId));
}
