using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Vendors.Commands.AddService;
using NestHub.Application.Vendors.Commands.ApproveVendor;
using NestHub.Application.Vendors.Commands.AwardTrustBadge;
using NestHub.Application.Vendors.Commands.RegisterVendor;
using NestHub.Application.Vendors.Commands.UpgradeVendorSubscription;
using NestHub.Application.Vendors.Dtos;
using NestHub.Application.Vendors.Queries.GetMyVendorProfile;
using NestHub.Application.Vendors.Queries.GetPendingVendorApprovals;
using NestHub.Application.Vendors.Queries.GetVendorProfile;
using NestHub.Application.Vendors.Queries.SearchVendors;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/vendors")]
public sealed class VendorsController : ControllerBase
{
    private readonly ISender _sender;

    public VendorsController(ISender sender) => _sender = sender;

    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] string? category, CancellationToken cancellationToken)
    {
        var vendors = await _sender.Send(new SearchVendorsQuery(query, category), cancellationToken);
        return Ok(vendors);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(Guid id, CancellationToken cancellationToken)
    {
        var vendor = await _sender.Send(new GetVendorProfileQuery(id), cancellationToken);
        return Ok(vendor);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var vendor = await _sender.Send(new GetMyVendorProfileQuery(userId), cancellationToken);
        return vendor is null ? NoContent() : Ok(vendor);
    }

    [HttpGet("pending-approval")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var vendors = await _sender.Send(new GetPendingVendorApprovalsQuery(), cancellationToken);
        return Ok(vendors);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(VendorDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterVendorCommand command, CancellationToken cancellationToken)
    {
        var vendor = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { id = vendor.Id }, vendor);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ApproveVendorCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/trust-badge")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AwardTrustBadge(Guid id, AwardTrustBadgeRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new AwardTrustBadgeCommand(id, request.Badge), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/upgrade-subscription")]
    [Authorize(Roles = "Vendor,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpgradeSubscription(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpgradeVendorSubscriptionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/services")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddService(Guid id, AddServiceRequestBody body, CancellationToken cancellationToken)
    {
        var command = new AddServiceCommand(id, body.Title, body.Description, body.Category, body.PricingType, body.Amount, body.Currency);
        var service = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProfile), new { id }, service);
    }
}

public sealed record AwardTrustBadgeRequest(Domain.Enums.TrustBadgeStatus Badge);

public sealed record AddServiceRequestBody(string Title, string Description, string Category, Domain.Enums.PricingType PricingType, decimal? Amount, string Currency = "INR");
