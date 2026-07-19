using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Residents.Commands.UpdateResident;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.Residents.Queries.GetAllResidents;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;
using NestHub.Application.Users.Commands.SetUserActive;

namespace NestHub.Admin.Pages.Residents;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AdminResidentDto> Residents { get; private set; } = Array.Empty<AdminResidentDto>();
    public IReadOnlyList<SocietyDto> AllSocieties { get; private set; } = Array.Empty<SocietyDto>();
    public Guid? ScopedSocietyId { get; private set; }

    public async Task OnGetAsync()
    {
        ScopedSocietyId = User.GetSocietyId();
        Residents = await _sender.Send(new GetAllResidentsQuery(ScopedSocietyId));

        if (ScopedSocietyId is null)
            AllSocieties = await _sender.Send(new GetActiveSocietiesQuery());
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid residentId, string name, string blockNumber, string flatNumber, Guid societyId)
    {
        var resident = (await _sender.Send(new GetAllResidentsQuery())).FirstOrDefault(r => r.Id == residentId);
        if (resident is null)
            return RedirectToPage();

        if (User.GetSocietyId() is { } scopedSocietyId && (scopedSocietyId != resident.SocietyId || scopedSocietyId != societyId))
            return RedirectToPage();

        await _sender.Send(new UpdateResidentCommand(residentId, name, blockNumber, flatNumber, societyId));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetActiveAsync(Guid residentId, Guid userId, bool isActive)
    {
        var resident = (await _sender.Send(new GetAllResidentsQuery())).FirstOrDefault(r => r.Id == residentId);
        if (resident is null)
            return RedirectToPage();

        if (User.GetSocietyId() is { } scopedSocietyId && scopedSocietyId != resident.SocietyId)
            return RedirectToPage();

        await _sender.Send(new SetUserActiveCommand(userId, isActive));
        return RedirectToPage();
    }
}
