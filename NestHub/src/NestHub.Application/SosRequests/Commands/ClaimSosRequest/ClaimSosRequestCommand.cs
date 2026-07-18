using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.SosRequests;

namespace NestHub.Application.SosRequests.Commands.ClaimSosRequest;

public sealed record ClaimSosRequestCommand(Guid SosRequestId, Guid VendorId) : IRequest<SosClaimDto>;

public sealed class ClaimSosRequestCommandValidator : AbstractValidator<ClaimSosRequestCommand>
{
    public ClaimSosRequestCommandValidator()
    {
        RuleFor(x => x.SosRequestId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class ClaimSosRequestCommandHandler : IRequestHandler<ClaimSosRequestCommand, SosClaimDto>
{
    private readonly ISosRequestRepository _sosRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ClaimSosRequestCommandHandler(
        ISosRequestRepository sosRequestRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _sosRequestRepository = sosRequestRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<SosClaimDto> Handle(ClaimSosRequestCommand request, CancellationToken cancellationToken)
    {
        var sosRequestId = new SosRequestId(request.SosRequestId);
        var sosRequest = await _sosRequestRepository.GetByIdAsync(sosRequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(SosRequest), request.SosRequestId);

        var vendorId = new VendorId(request.VendorId);
        var claim = sosRequest.ClaimBy(vendorId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.NotifySosRequestClaimedAsync(sosRequestId, sosRequest.ResidentId, vendorId, cancellationToken);

        return claim.ToDto();
    }
}
