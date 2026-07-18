using FluentValidation;
using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.SosRequests;

namespace NestHub.Application.SosRequests.Commands.CreateSosRequest;

public sealed record CreateSosRequestCommand(Guid ResidentId, Guid SocietyId, string Category, string Description) : IRequest<SosRequestDto>;

public sealed class CreateSosRequestCommandValidator : AbstractValidator<CreateSosRequestCommand>
{
    public CreateSosRequestCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
    }
}

public sealed class CreateSosRequestCommandHandler : IRequestHandler<CreateSosRequestCommand, SosRequestDto>
{
    private readonly ISosRequestRepository _sosRequestRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public CreateSosRequestCommandHandler(
        ISosRequestRepository sosRequestRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService)
    {
        _sosRequestRepository = sosRequestRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<SosRequestDto> Handle(CreateSosRequestCommand request, CancellationToken cancellationToken)
    {
        var societyId = new SocietyId(request.SocietyId);
        var sosRequest = SosRequest.RaiseNew(new ResidentId(request.ResidentId), societyId, request.Category, request.Description);

        _sosRequestRepository.Add(sosRequest);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.BroadcastSosRequestAsync(sosRequest.Id, societyId, sosRequest.Category, sosRequest.Description, cancellationToken);

        return sosRequest.ToDto();
    }
}
