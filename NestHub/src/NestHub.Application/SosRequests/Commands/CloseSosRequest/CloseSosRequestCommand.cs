using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.SosRequests;

namespace NestHub.Application.SosRequests.Commands.CloseSosRequest;

public sealed record CloseSosRequestCommand(Guid SosRequestId) : IRequest<Unit>;

public sealed class CloseSosRequestCommandValidator : AbstractValidator<CloseSosRequestCommand>
{
    public CloseSosRequestCommandValidator() => RuleFor(x => x.SosRequestId).NotEmpty();
}

public sealed class CloseSosRequestCommandHandler : IRequestHandler<CloseSosRequestCommand, Unit>
{
    private readonly ISosRequestRepository _sosRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseSosRequestCommandHandler(ISosRequestRepository sosRequestRepository, IUnitOfWork unitOfWork)
    {
        _sosRequestRepository = sosRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(CloseSosRequestCommand request, CancellationToken cancellationToken)
    {
        var sosRequest = await _sosRequestRepository.GetByIdAsync(new SosRequestId(request.SosRequestId), cancellationToken)
            ?? throw new NotFoundException(nameof(SosRequest), request.SosRequestId);

        sosRequest.Close();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
