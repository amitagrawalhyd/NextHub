using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly NestHubDbContext _context;

    public UserRepository(NestHubDbContext context) => _context = context;

    public Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);

    public Task<bool> ExistsByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default) =>
        _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Users.ToListAsync(cancellationToken);

    public void Add(User user) => _context.Users.Add(user);
}
