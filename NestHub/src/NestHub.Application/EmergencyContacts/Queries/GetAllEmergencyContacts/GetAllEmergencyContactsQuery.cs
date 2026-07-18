using MediatR;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.EmergencyContacts.Queries.GetAllEmergencyContacts;

public sealed record GetAllEmergencyContactsQuery(Guid? SocietyId = null) : IRequest<IReadOnlyList<AdminEmergencyContactDto>>;

public sealed class GetAllEmergencyContactsQueryHandler : IRequestHandler<GetAllEmergencyContactsQuery, IReadOnlyList<AdminEmergencyContactDto>>
{
    private readonly IEmergencyContactRepository _emergencyContactRepository;
    private readonly ISocietyRepository _societyRepository;

    public GetAllEmergencyContactsQueryHandler(IEmergencyContactRepository emergencyContactRepository, ISocietyRepository societyRepository)
    {
        _emergencyContactRepository = emergencyContactRepository;
        _societyRepository = societyRepository;
    }

    public async Task<IReadOnlyList<AdminEmergencyContactDto>> Handle(GetAllEmergencyContactsQuery request, CancellationToken cancellationToken)
    {
        var contacts = await _emergencyContactRepository.GetAllAsync(cancellationToken);
        var societies = (await _societyRepository.GetActiveAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);

        if (request.SocietyId is { } societyId)
            contacts = contacts.Where(c => c.SocietyId.Value == societyId).ToList();

        return contacts
            .Select(c => new AdminEmergencyContactDto(
                c.Id.Value,
                societies.TryGetValue(c.SocietyId, out var name) ? name : "(unknown)",
                c.Name,
                c.Role,
                c.PhoneNumber))
            .ToList();
    }
}
