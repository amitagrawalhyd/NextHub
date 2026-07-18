using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Categories;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Categories.Commands.SetCategoryActive;

public sealed record SetCategoryActiveCommand(Guid CategoryId, bool IsActive) : IRequest<Unit>;

public sealed class SetCategoryActiveCommandValidator : AbstractValidator<SetCategoryActiveCommand>
{
    public SetCategoryActiveCommandValidator() => RuleFor(x => x.CategoryId).NotEmpty();
}

public sealed class SetCategoryActiveCommandHandler : IRequestHandler<SetCategoryActiveCommand, Unit>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetCategoryActiveCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetCategoryActiveCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(new CategoryId(request.CategoryId), cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        if (request.IsActive) category.Activate(); else category.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
