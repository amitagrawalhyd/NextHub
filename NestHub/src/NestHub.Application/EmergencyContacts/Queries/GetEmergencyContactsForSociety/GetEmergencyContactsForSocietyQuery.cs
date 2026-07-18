using MediatR;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.EmergencyContacts.Queries.GetEmergencyContactsForSociety;

public sealed record GetEmergencyContactsForSocietyQuery(Guid SocietyId) : IRequest<IReadOnlyList<EmergencyContactDto>>;

public sealed class GetEmergencyContactsForSocietyQueryHandler : IRequestHandler<GetEmergencyContactsForSocietyQuery, IReadOnlyList<EmergencyContactDto>>
{
    private readonly IEmergencyContactRepository _emergencyContactRepository;

    public GetEmergencyContactsForSocietyQueryHandler(IEmergencyContactRepository emergencyContactRepository) => _emergencyContactRepository = emergencyContactRepository;

    public async Task<IReadOnlyList<EmergencyContactDto>> Handle(GetEmergencyContactsForSocietyQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _emergencyContactRepository.GetForSocietyAsync(new SocietyId(request.SocietyId), cancellationToken);
        return contacts.Select(c => new EmergencyContactDto(c.Id.Value, c.SocietyId.Value, c.Name, c.Role, c.PhoneNumber)).ToList();
    }
}
