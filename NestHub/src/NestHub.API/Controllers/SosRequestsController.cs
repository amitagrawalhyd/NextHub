using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.SosRequests.Commands.ClaimSosRequest;
using NestHub.Application.SosRequests.Commands.CloseSosRequest;
using NestHub.Application.SosRequests.Commands.CreateSosRequest;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Application.SosRequests.Queries.GetOpenSosRequestsForVendor;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/sos-requests")]
[Authorize]
public sealed class SosRequestsController : ControllerBase
{
    private readonly ISender _sender;

    public SosRequestsController(ISender sender) => _sender = sender;

    [HttpGet("open")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(IReadOnlyList<SosRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenForVendor([FromQuery] Guid societyId, [FromQuery] string category, CancellationToken cancellationToken)
    {
        var requests = await _sender.Send(new GetOpenSosRequestsForVendorQuery(societyId, category), cancellationToken);
        return Ok(requests);
    }

    [HttpPost]
    [Authorize(Roles = "Resident")]
    [ProducesResponseType(typeof(SosRequestDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CreateSosRequestCommand command, CancellationToken cancellationToken)
    {
        var sosRequest = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetOpenForVendor), new { societyId = sosRequest.SocietyId, category = sosRequest.Category }, sosRequest);
    }

    [HttpPost("{id:guid}/claim")]
    [Authorize(Roles = "Vendor")]
    [ProducesResponseType(typeof(SosClaimDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Claim(Guid id, ClaimSosRequestRequestBody body, CancellationToken cancellationToken)
    {
        var claim = await _sender.Send(new ClaimSosRequestCommand(id, body.VendorId), cancellationToken);
        return Ok(claim);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Resident,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new CloseSosRequestCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record ClaimSosRequestRequestBody(Guid VendorId);
