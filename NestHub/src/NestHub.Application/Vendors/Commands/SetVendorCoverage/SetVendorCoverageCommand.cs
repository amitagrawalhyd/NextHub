using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Commands.SetVendorCoverage;

public sealed record SetVendorCoverageCommand(Guid VendorId, IReadOnlyList<Guid> SocietyIds) : IRequest<Unit>;

public sealed class SetVendorCoverageCommandValidator : AbstractValidator<SetVendorCoverageCommand>
{
    public SetVendorCoverageCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class SetVendorCoverageCommandHandler : IRequestHandler<SetVendorCoverageCommand, Unit>
{
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetVendorCoverageCommandHandler(IVendorSocietyCoverageRepository coverageRepository, IUnitOfWork unitOfWork)
    {
        _coverageRepository = coverageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetVendorCoverageCommand request, CancellationToken cancellationToken)
    {
        await _coverageRepository.ReplaceForVendorAsync(
            new VendorId(request.VendorId),
            request.SocietyIds.Select(id => new SocietyId(id)),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
