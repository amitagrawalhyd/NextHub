using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Reviews;

namespace NestHub.Application.Reviews.Commands.FlagReview;

public sealed record FlagReviewCommand(Guid ReviewId) : IRequest<Unit>;

public sealed class FlagReviewCommandValidator : AbstractValidator<FlagReviewCommand>
{
    public FlagReviewCommandValidator() => RuleFor(x => x.ReviewId).NotEmpty();
}

public sealed class FlagReviewCommandHandler : IRequestHandler<FlagReviewCommand, Unit>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FlagReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(FlagReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(new ReviewId(request.ReviewId), cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        review.Flag();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
