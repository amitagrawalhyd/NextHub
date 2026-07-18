using FluentValidation;
using MediatR;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.EmergencyContacts;
using NestHub.Domain.Repositories;

namespace NestHub.Application.EmergencyContacts.Commands.CreateEmergencyContact;

public sealed record CreateEmergencyContactCommand(Guid SocietyId, string Name, string Role, string PhoneNumber) : IRequest<EmergencyContactDto>;

public sealed class CreateEmergencyContactCommandValidator : AbstractValidator<CreateEmergencyContactCommand>
{
    public CreateEmergencyContactCommandValidator()
    {
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.PhoneNumber).NotEmpty();
    }
}

public sealed class CreateEmergencyContactCommandHandler : IRequestHandler<CreateEmergencyContactCommand, EmergencyContactDto>
{
    private readonly IEmergencyContactRepository _emergencyContactRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmergencyContactCommandHandler(IEmergencyContactRepository emergencyContactRepository, IUnitOfWork unitOfWork)
    {
        _emergencyContactRepository = emergencyContactRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmergencyContactDto> Handle(CreateEmergencyContactCommand request, CancellationToken cancellationToken)
    {
        var contact = EmergencyContact.Create(new SocietyId(request.SocietyId), request.Name, request.Role, request.PhoneNumber);

        _emergencyContactRepository.Add(contact);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new EmergencyContactDto(contact.Id.Value, contact.SocietyId.Value, contact.Name, contact.Role, contact.PhoneNumber);
    }
}
