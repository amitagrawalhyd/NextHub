using FluentValidation;
using MediatR;
using NestHub.Application.Categories.Dtos;
using NestHub.Domain.Categories;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : IRequest<CategoryDto>;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator() => RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
}

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await _categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
            throw new InvalidOperationException($"Category '{request.Name}' already exists.");

        var category = Category.Create(request.Name);
        _categoryRepository.Add(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id.Value, category.Name, category.IsActive);
    }
}
