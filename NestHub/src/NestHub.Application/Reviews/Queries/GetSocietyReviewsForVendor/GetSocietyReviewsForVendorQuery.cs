using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Reviews.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Reviews.Queries.GetSocietyReviewsForVendor;

/// <summary>
/// Powers the "Neighbor-Verified Reviews" feed: reviews for a vendor are filtered to the
/// requesting resident's own society, so only neighbors' opinions are shown.
/// </summary>
public sealed record GetSocietyReviewsForVendorQuery(Guid VendorId, Guid SocietyId) : IRequest<IReadOnlyList<ReviewDto>>;

public sealed class GetSocietyReviewsForVendorQueryHandler : IRequestHandler<GetSocietyReviewsForVendorQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetSocietyReviewsForVendorQueryHandler(IReviewRepository reviewRepository) => _reviewRepository = reviewRepository;

    public async Task<IReadOnlyList<ReviewDto>> Handle(GetSocietyReviewsForVendorQuery request, CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByVendorAndSocietyAsync(
            new VendorId(request.VendorId),
            new SocietyId(request.SocietyId),
            cancellationToken);

        return reviews.Where(r => !r.IsFlagged).Select(r => r.ToDto()).ToList();
    }
}
