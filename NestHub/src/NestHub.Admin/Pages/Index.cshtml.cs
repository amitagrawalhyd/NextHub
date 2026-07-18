using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Admin.Queries.GetAdminDashboardSummary;
using NestHub.Application.Analytics.Dtos;
using NestHub.Application.Analytics.Queries.GetTopVendorsByEngagement;

namespace NestHub.Admin.Pages;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public AdminDashboardSummaryDto Summary { get; private set; } = null!;
    public IReadOnlyList<TopVendorDto> TopVendorsByEngagement { get; private set; } = Array.Empty<TopVendorDto>();

    public async Task OnGetAsync()
    {
        var societyId = User.GetSocietyId();
        Summary = await _sender.Send(new GetAdminDashboardSummaryQuery(societyId));

        if (societyId is null)
            TopVendorsByEngagement = await _sender.Send(new GetTopVendorsByEngagementQuery(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow, 5));
    }
}
