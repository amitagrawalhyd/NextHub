using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Application.Vendors.Commands.ApproveVendor;
using NestHub.Application.Vendors.Commands.AwardTrustBadge;
using NestHub.Application.Vendors.Dtos;
using NestHub.Application.Vendors.Queries.GetPendingVendorApprovals;
using NestHub.Application.Vendors.Queries.SearchVendors;
using NestHub.Domain.Enums;

namespace NestHub.Admin.Pages.Vendors;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<VendorDto> PendingVendors { get; private set; } = Array.Empty<VendorDto>();
    public IReadOnlyList<VendorDto> ApprovedVendors { get; private set; } = Array.Empty<VendorDto>();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostApproveAsync(Guid vendorId)
    {
        await _sender.Send(new ApproveVendorCommand(vendorId));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAwardBadgeAsync(Guid vendorId, TrustBadgeStatus badge)
    {
        await _sender.Send(new AwardTrustBadgeCommand(vendorId, badge));
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        PendingVendors = await _sender.Send(new GetPendingVendorApprovalsQuery());
        var allApproved = await _sender.Send(new SearchVendorsQuery(null, null));
        ApprovedVendors = allApproved.Where(v => v.IsApproved).ToList();
    }
}
