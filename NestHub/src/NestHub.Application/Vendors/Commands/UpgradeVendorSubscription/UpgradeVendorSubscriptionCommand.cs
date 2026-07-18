using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.UpgradeVendorSubscription;

public sealed record UpgradeVendorSubscriptionCommand(Guid VendorId) : IRequest<Unit>;

public sealed class UpgradeVendorSubscriptionCommandValidator : AbstractValidator<UpgradeVendorSubscriptionCommand>
{
    public UpgradeVendorSubscriptionCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class UpgradeVendorSubscriptionCommandHandler : IRequestHandler<UpgradeVendorSubscriptionCommand, Unit>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpgradeVendorSubscriptionCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpgradeVendorSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        vendor.UpgradeToPremium();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
