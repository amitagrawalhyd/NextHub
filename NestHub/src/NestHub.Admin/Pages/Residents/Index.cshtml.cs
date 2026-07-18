using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.Residents.Queries.GetAllResidents;

namespace NestHub.Admin.Pages.Residents;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AdminResidentDto> Residents { get; private set; } = Array.Empty<AdminResidentDto>();

    public async Task OnGetAsync()
    {
        Residents = await _sender.Send(new GetAllResidentsQuery(User.GetSocietyId()));
    }
}
