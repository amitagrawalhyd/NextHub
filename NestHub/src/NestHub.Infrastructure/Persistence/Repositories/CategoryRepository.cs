using Microsoft.EntityFrameworkCore;
using NestHub.Domain.Categories;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly NestHubDbContext _context;

    public CategoryRepository(NestHubDbContext context) => _context = context;

    public Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default) =>
        _context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Categories.OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default) =>
        _context.Categories.AnyAsync(c => c.Name == name, cancellationToken);

    public void Add(Category category) => _context.Categories.Add(category);

    public void Remove(Category category) => _context.Categories.Remove(category);
}
