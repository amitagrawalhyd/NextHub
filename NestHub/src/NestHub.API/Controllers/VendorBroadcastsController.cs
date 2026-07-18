using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Residents.Queries.GetMyResidentProfile;
using NestHub.Application.Vendors.Commands.CreateVendorBroadcast;
using NestHub.Application.Vendors.Commands.DeleteVendorBroadcast;
using NestHub.Application.Vendors.Dtos;
using NestHub.Application.Vendors.Queries.GetActiveBroadcastsForVendor;
using NestHub.Application.Vendors.Queries.GetMyVendorBroadcasts;
using NestHub.Application.Vendors.Queries.GetVendorBroadcastFeedForResident;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/vendor-broadcasts")]
[Authorize]
public sealed class VendorBroadcastsController : ControllerBase
{
    private readonly ISender _sender;

    public VendorBroadcastsController(ISender sender) => _sender = sender;

    [HttpGet("feed")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorBroadcastDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeed([FromQuery] Guid societyId, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resident = await _sender.Send(new GetMyResidentProfileQuery(userId), cancellationToken);

        var broadcasts = await _sender.Send(new GetVendorBroadcastFeedForResidentQuery(societyId, resident?.Id), cancellationToken);
        return Ok(broadcasts);
    }

    [HttpGet("vendor/{vendorId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorBroadcastDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForVendor(Guid vendorId, CancellationToken cancellationToken)
    {
        var broadcasts = await _sender.Send(new GetActiveBroadcastsForVendorQuery(vendorId), cancellationToken);
        return Ok(broadcasts);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorBroadcastDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] Guid vendorId, CancellationToken cancellationToken)
    {
        var broadcasts = await _sender.Send(new GetMyVendorBroadcastsQuery(vendorId), cancellationToken);
        return Ok(broadcasts);
    }

    [HttpPost]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(VendorBroadcastDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateVendorBroadcastCommand command, CancellationToken cancellationToken)
    {
        var broadcast = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { vendorId = broadcast.VendorId }, broadcast);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteVendorBroadcastCommand(id), cancellationToken);
        return NoContent();
    }
}
