using FluentValidation;
using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Commands.UnmuteVendor;

public sealed record UnmuteVendorCommand(Guid ResidentId, Guid VendorId) : IRequest<Unit>;

public sealed class UnmuteVendorCommandValidator : AbstractValidator<UnmuteVendorCommand>
{
    public UnmuteVendorCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class UnmuteVendorCommandHandler : IRequestHandler<UnmuteVendorCommand, Unit>
{
    private readonly IVendorMuteRepository _muteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UnmuteVendorCommandHandler(IVendorMuteRepository muteRepository, IUnitOfWork unitOfWork)
    {
        _muteRepository = muteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UnmuteVendorCommand request, CancellationToken cancellationToken)
    {
        var mute = await _muteRepository.GetAsync(new ResidentId(request.ResidentId), new VendorId(request.VendorId), cancellationToken);
        if (mute is not null)
        {
            _muteRepository.Remove(mute);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
