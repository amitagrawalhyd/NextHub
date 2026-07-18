using NestHub.Domain.Categories;
using NestHub.Domain.Common;

namespace NestHub.Domain.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(CategoryId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    void Add(Category category);
    void Remove(Category category);
}
