using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.AddFavorite;

public sealed record AddFavoriteCommand(Guid ResidentId, Guid VendorId) : IRequest<Unit>;

public sealed class AddFavoriteCommandValidator : AbstractValidator<AddFavoriteCommand>
{
    public AddFavoriteCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class AddFavoriteCommandHandler : IRequestHandler<AddFavoriteCommand, Unit>
{
    private readonly IVendorFavoriteRepository _favoriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddFavoriteCommandHandler(IVendorFavoriteRepository favoriteRepository, IUnitOfWork unitOfWork)
    {
        _favoriteRepository = favoriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var residentId = new ResidentId(request.ResidentId);
        var vendorId = new VendorId(request.VendorId);

        var existing = await _favoriteRepository.GetAsync(residentId, vendorId, cancellationToken);
        if (existing is null)
        {
            _favoriteRepository.Add(VendorFavorite.Create(residentId, vendorId));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
