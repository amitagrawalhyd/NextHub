using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(Guid ResidentId, Guid VendorId) : IRequest<Unit>;

public sealed class RemoveFavoriteCommandValidator : AbstractValidator<RemoveFavoriteCommand>
{
    public RemoveFavoriteCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class RemoveFavoriteCommandHandler : IRequestHandler<RemoveFavoriteCommand, Unit>
{
    private readonly IVendorFavoriteRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFavoriteCommandHandler(IVendorFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var favorite = await _favoriteRepository.GetAsync(new ResidentId(request.ResidentId), new VendorId(request.VendorId), cancellationToken);
        if (favorite is not null)
        {
            _favoriteRepository.Remove(favorite);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
