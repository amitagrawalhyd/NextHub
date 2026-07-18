using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.MuteVendor;

public sealed record MuteVendorCommand(Guid ResidentId, Guid VendorId) : IRequest<Unit>;

public sealed class MuteVendorCommandValidator : AbstractValidator<MuteVendorCommand>
{
    public MuteVendorCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class MuteVendorCommandHandler : IRequestHandler<MuteVendorCommand, Unit>
{
    private readonly IVendorMuteRepository _muteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MuteVendorCommandHandler(IVendorMuteRepository muteRepository, IUnitOfWork unitOfWork)
    {
        _muteRepository = muteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MuteVendorCommand request, CancellationToken cancellationToken)
    {
        var residentId = new ResidentId(request.ResidentId);
        var vendorId = new VendorId(request.VendorId);

        var existing = await _muteRepository.GetAsync(residentId, vendorId, cancellationToken);
        if (existing is null)
        {
            _muteRepository.Add(VendorMute.Create(residentId, vendorId));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
