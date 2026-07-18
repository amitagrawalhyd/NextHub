using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Announcements.Commands.CreateAnnouncement;
using NestHub.Application.Announcements.Commands.DeleteAnnouncement;
using NestHub.Application.Announcements.Dtos;
using NestHub.Application.Announcements.Queries.GetAllAnnouncements;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;

namespace NestHub.Admin.Pages.Announcements;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AdminAnnouncementDto> Announcements { get; private set; } = Array.Empty<AdminAnnouncementDto>();
    public IReadOnlyList<SocietyDto> Societies { get; private set; } = Array.Empty<SocietyDto>();
    public Guid? ScopedSocietyId { get; private set; }

    [BindProperty]
    public Guid NewSocietyId { get; set; }

    [BindProperty]
    public string NewTitle { get; set; } = string.Empty;

    [BindProperty]
    public string NewBody { get; set; } = string.Empty;

    [BindProperty]
    public DateTime? NewExpiresAtUtc { get; set; }

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var societyId = User.GetSocietyId() ?? NewSocietyId;

        if (societyId != Guid.Empty && !string.IsNullOrWhiteSpace(NewTitle) && !string.IsNullOrWhiteSpace(NewBody))
            await _sender.Send(new CreateAnnouncementCommand(societyId, NewTitle, NewBody, NewExpiresAtUtc));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid announcementId)
    {
        await _sender.Send(new DeleteAnnouncementCommand(announcementId));
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        ScopedSocietyId = User.GetSocietyId();
        Announcements = await _sender.Send(new GetAllAnnouncementsQuery(ScopedSocietyId));
        Societies = await _sender.Send(new GetActiveSocietiesQuery());
    }
}
