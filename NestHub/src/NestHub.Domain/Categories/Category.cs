using NestHub.Domain.Common;

namespace NestHub.Domain.Categories;

public sealed class Category : AggregateRoot<CategoryId>
{
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Category()
    {
    }

    private Category(CategoryId id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));

        return new Category(CategoryId.New(), name.Trim());
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.", nameof(name));
        Name = name.Trim();
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
