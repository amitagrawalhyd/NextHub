using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.AwardTrustBadge;

public sealed record AwardTrustBadgeCommand(Guid VendorId, TrustBadgeStatus Badge) : IRequest<Unit>;

public sealed class AwardTrustBadgeCommandValidator : AbstractValidator<AwardTrustBadgeCommand>
{
    public AwardTrustBadgeCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.Badge).IsInEnum();
    }
}

public sealed class AwardTrustBadgeCommandHandler : IRequestHandler<AwardTrustBadgeCommand, Unit>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AwardTrustBadgeCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AwardTrustBadgeCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        vendor.AwardTrustBadge(request.Badge);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
