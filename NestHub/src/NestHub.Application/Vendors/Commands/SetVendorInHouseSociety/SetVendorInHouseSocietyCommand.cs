using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;

namespace NestHub.Application.Vendors.Commands.SetVendorInHouseSociety;

public sealed record SetVendorInHouseSocietyCommand(Guid VendorId, Guid? SocietyId) : IRequest<Unit>;

public sealed class SetVendorInHouseSocietyCommandValidator : AbstractValidator<SetVendorInHouseSocietyCommand>
{
    public SetVendorInHouseSocietyCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class SetVendorInHouseSocietyCommandHandler : IRequestHandler<SetVendorInHouseSocietyCommand, Unit>
{
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetVendorInHouseSocietyCommandHandler(
        IVendorSocietyCoverageRepository coverageRepository,
        ISocietyRepository societyRepository,
        IUnitOfWork unitOfWork)
    {
        _coverageRepository = coverageRepository;
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetVendorInHouseSocietyCommand request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);

        SocietyId? societyId = null;
        if (request.SocietyId is { } rawSocietyId)
        {
            societyId = new SocietyId(rawSocietyId);
            _ = await _societyRepository.GetByIdAsync(societyId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Society), rawSocietyId);
        }

        await _coverageRepository.SetInHouseAsync(vendorId, societyId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
