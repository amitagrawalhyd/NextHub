using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Reviews.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Reviews;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Reviews.Commands.SubmitReview;

public sealed record SubmitReviewCommand(Guid ResidentId, Guid VendorId, Guid SocietyId, int Rating, string? Comment) : IRequest<ReviewDto>;

public sealed class SubmitReviewCommandValidator : AbstractValidator<SubmitReviewCommand>
{
    public SubmitReviewCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}

public sealed class SubmitReviewCommandHandler : IRequestHandler<SubmitReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAiService _aiService;

    public SubmitReviewCommandHandler(
        IReviewRepository reviewRepository,
        IVendorRepository vendorRepository,
        IUnitOfWork unitOfWork,
        IAiService aiService)
    {
        _reviewRepository = reviewRepository;
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
        _aiService = aiService;
    }

    public async Task<ReviewDto> Handle(SubmitReviewCommand request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var review = Review.Submit(
            new ResidentId(request.ResidentId),
            vendorId,
            new SocietyId(request.SocietyId),
            Rating.Create(request.Rating),
            request.Comment);

        if (!string.IsNullOrWhiteSpace(request.Comment))
            review.ApplySentimentScore(_aiService.ScoreSentiment(request.Comment));

        _reviewRepository.Add(review);

        var existingReviews = await _reviewRepository.GetByVendorAsync(vendorId, cancellationToken);
        var newAverage = existingReviews.Select(r => (double)r.Rating.Value).Append(review.Rating.Value).Average();
        vendor.RecalculateAverageRating(Math.Round(newAverage, 2));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return review.ToDto();
    }
}
