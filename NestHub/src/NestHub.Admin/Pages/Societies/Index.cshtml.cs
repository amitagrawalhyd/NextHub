using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Societies.Commands.RegisterSociety;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;

namespace NestHub.Admin.Pages.Societies;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<SocietyDto> Societies { get; private set; } = Array.Empty<SocietyDto>();
    public Guid? ScopedSocietyId { get; private set; }

    [BindProperty]
    public RegisterSocietyCommand NewSociety { get; set; } = new(string.Empty, string.Empty, null, null);

    public async Task OnGetAsync()
    {
        ScopedSocietyId = User.GetSocietyId();
        var societies = await _sender.Send(new GetActiveSocietiesQuery());
        Societies = ScopedSocietyId is { } societyId
            ? societies.Where(s => s.Id == societyId).ToList()
            : societies;
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (User.GetSocietyId() is not null)
            return RedirectToPage();

        if (!string.IsNullOrWhiteSpace(NewSociety.Name) && !string.IsNullOrWhiteSpace(NewSociety.Address))
            await _sender.Send(NewSociety);

        return RedirectToPage();
    }
}
