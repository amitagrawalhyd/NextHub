using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Application.Audit.Dtos;
using NestHub.Application.Audit.Queries.GetSystemAudit;

namespace NestHub.Admin.Pages.Audit;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AuditEntryDto> Entries { get; private set; } = Array.Empty<AuditEntryDto>();

    public async Task OnGetAsync() => Entries = await _sender.Send(new GetSystemAuditQuery());
}
