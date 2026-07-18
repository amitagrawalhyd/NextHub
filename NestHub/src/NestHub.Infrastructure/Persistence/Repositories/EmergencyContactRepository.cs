using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.EmergencyContacts;
using NestHub.Domain.Repositories;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class EmergencyContactRepository : IEmergencyContactRepository
{
    private readonly NestHubDbContext _context;

    public EmergencyContactRepository(NestHubDbContext context) => _context = context;

    public Task<EmergencyContact?> GetByIdAsync(EmergencyContactId id, CancellationToken cancellationToken = default) =>
        _context.EmergencyContacts.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<EmergencyContact>> GetForSocietyAsync(SocietyId societyId, CancellationToken cancellationToken = default) =>
        await _context.EmergencyContacts.Where(c => c.SocietyId == societyId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<EmergencyContact>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.EmergencyContacts.ToListAsync(cancellationToken);

    public void Add(EmergencyContact contact) => _context.EmergencyContacts.Add(contact);

    public void Remove(EmergencyContact contact) => _context.EmergencyContacts.Remove(contact);
}
