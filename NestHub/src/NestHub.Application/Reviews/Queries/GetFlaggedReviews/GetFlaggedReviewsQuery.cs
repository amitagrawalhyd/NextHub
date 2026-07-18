using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Reviews.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Reviews.Queries.GetFlaggedReviews;

public sealed record GetFlaggedReviewsQuery : IRequest<IReadOnlyList<ReviewDto>>;

public sealed class GetFlaggedReviewsQueryHandler : IRequestHandler<GetFlaggedReviewsQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetFlaggedReviewsQueryHandler(IReviewRepository reviewRepository) => _reviewRepository = reviewRepository;

    public async Task<IReadOnlyList<ReviewDto>> Handle(GetFlaggedReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetFlaggedAsync(cancellationToken);
        return reviews.Select(r => r.ToDto()).ToList();
    }
}
