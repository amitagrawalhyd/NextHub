using NestHub.Domain.Common;
using NestHub.Domain.EmergencyContacts;

namespace NestHub.Domain.Repositories;

public interface IEmergencyContactRepository
{
    Task<EmergencyContact?> GetByIdAsync(EmergencyContactId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmergencyContact>> GetForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmergencyContact>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(EmergencyContact contact);
    void Remove(EmergencyContact contact);
}
