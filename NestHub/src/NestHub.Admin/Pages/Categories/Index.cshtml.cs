using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Application.Categories.Commands.CreateCategory;
using NestHub.Application.Categories.Commands.DeleteCategory;
using NestHub.Application.Categories.Commands.SetCategoryActive;
using NestHub.Application.Categories.Dtos;
using NestHub.Application.Categories.Queries.GetCategories;

namespace NestHub.Admin.Pages.Categories;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<CategoryDto> Categories { get; private set; } = Array.Empty<CategoryDto>();

    [BindProperty]
    public string NewCategoryName { get; set; } = string.Empty;

    public async Task OnGetAsync() => Categories = await _sender.Send(new GetCategoriesQuery());

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewCategoryName))
            await _sender.Send(new CreateCategoryCommand(NewCategoryName));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid categoryId, bool isActive)
    {
        await _sender.Send(new SetCategoryActiveCommand(categoryId, !isActive));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid categoryId)
    {
        await _sender.Send(new DeleteCategoryCommand(categoryId));
        return RedirectToPage();
    }
}
