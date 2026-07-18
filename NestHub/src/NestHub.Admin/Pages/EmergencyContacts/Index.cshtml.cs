using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.EmergencyContacts.Commands.CreateEmergencyContact;
using NestHub.Application.EmergencyContacts.Commands.DeleteEmergencyContact;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Application.EmergencyContacts.Queries.GetAllEmergencyContacts;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;

namespace NestHub.Admin.Pages.EmergencyContacts;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AdminEmergencyContactDto> Contacts { get; private set; } = Array.Empty<AdminEmergencyContactDto>();
    public IReadOnlyList<SocietyDto> Societies { get; private set; } = Array.Empty<SocietyDto>();
    public Guid? ScopedSocietyId { get; private set; }

    [BindProperty]
    public Guid NewSocietyId { get; set; }

    [BindProperty]
    public string NewName { get; set; } = string.Empty;

    [BindProperty]
    public string NewRole { get; set; } = string.Empty;

    [BindProperty]
    public string NewPhoneNumber { get; set; } = string.Empty;

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var societyId = User.GetSocietyId() ?? NewSocietyId;

        if (societyId != Guid.Empty && !string.IsNullOrWhiteSpace(NewName) && !string.IsNullOrWhiteSpace(NewPhoneNumber))
            await _sender.Send(new CreateEmergencyContactCommand(societyId, NewName, NewRole, NewPhoneNumber));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid emergencyContactId)
    {
        await _sender.Send(new DeleteEmergencyContactCommand(emergencyContactId));
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        ScopedSocietyId = User.GetSocietyId();
        Contacts = await _sender.Send(new GetAllEmergencyContactsQuery(ScopedSocietyId));
        Societies = await _sender.Send(new GetActiveSocietiesQuery());
    }
}
