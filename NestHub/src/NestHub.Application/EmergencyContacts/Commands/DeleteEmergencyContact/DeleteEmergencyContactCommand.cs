using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.EmergencyContacts;
using NestHub.Domain.Repositories;

namespace NestHub.Application.EmergencyContacts.Commands.DeleteEmergencyContact;

public sealed record DeleteEmergencyContactCommand(Guid EmergencyContactId) : IRequest<Unit>;

public sealed class DeleteEmergencyContactCommandValidator : AbstractValidator<DeleteEmergencyContactCommand>
{
    public DeleteEmergencyContactCommandValidator() => RuleFor(x => x.EmergencyContactId).NotEmpty();
}

public sealed class DeleteEmergencyContactCommandHandler : IRequestHandler<DeleteEmergencyContactCommand, Unit>
{
    private readonly IEmergencyContactRepository _emergencyContactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmergencyContactCommandHandler(IEmergencyContactRepository emergencyContactRepository, IUnitOfWork unitOfWork)
    {
        _emergencyContactRepository = emergencyContactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteEmergencyContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _emergencyContactRepository.GetByIdAsync(new EmergencyContactId(request.EmergencyContactId), cancellationToken)
            ?? throw new NotFoundException(nameof(EmergencyContact), request.EmergencyContactId);

        _emergencyContactRepository.Remove(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
