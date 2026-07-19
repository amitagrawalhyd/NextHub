using MediatR;
using Microsoft.AspNetCore.Mvc;
using NestHub.Admin.Common;
using NestHub.Application.Vendors.Commands.ApproveVendor;
using NestHub.Application.Vendors.Commands.AwardTrustBadge;
using NestHub.Application.Vendors.Commands.SetVendorFeatured;
using NestHub.Application.Vendors.Commands.UpdateVendorProfile;
using NestHub.Application.Vendors.Commands.UpgradeVendorSubscription;
using NestHub.Application.Vendors.Dtos;
using NestHub.Application.Vendors.Queries.GetPendingVendorApprovals;
using NestHub.Application.Vendors.Queries.SearchVendors;
using NestHub.Domain.Enums;

namespace NestHub.Admin.Pages.Vendors;

public sealed class IndexModel : CentralAdminOnlyPageModel
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

    public async Task<IActionResult> OnPostUpgradeSubscriptionAsync(Guid vendorId)
    {
        await _sender.Send(new UpgradeVendorSubscriptionCommand(vendorId));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetFeaturedAsync(Guid vendorId, bool isFeatured)
    {
        await _sender.Send(new SetVendorFeaturedCommand(vendorId, isFeatured));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid vendorId, string businessName, string? bio, string? logoUrl, string whatsAppNumber)
    {
        await _sender.Send(new UpdateVendorProfileCommand(vendorId, businessName, bio, logoUrl, whatsAppNumber));
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        PendingVendors = await _sender.Send(new GetPendingVendorApprovalsQuery());
        var allApproved = await _sender.Send(new SearchVendorsQuery(null, null));
        ApprovedVendors = allApproved.Where(v => v.IsApproved).ToList();
    }
}
