using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Reviews;

namespace NestHub.Application.Reviews.Commands.RemoveReview;

public sealed record RemoveReviewCommand(Guid ReviewId) : IRequest<Unit>;

public sealed class RemoveReviewCommandValidator : AbstractValidator<RemoveReviewCommand>
{
    public RemoveReviewCommandValidator() => RuleFor(x => x.ReviewId).NotEmpty();
}

public sealed class RemoveReviewCommandHandler : IRequestHandler<RemoveReviewCommand, Unit>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveReviewCommandHandler(IReviewRepository reviewRepository, IUnitOfWork unitOfWork)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(new ReviewId(request.ReviewId), cancellationToken)
            ?? throw new NotFoundException(nameof(Review), request.ReviewId);

        review.MarkRemoved();
        _reviewRepository.Remove(review);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
