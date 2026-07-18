using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Residents.Commands.RegisterResident;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.Residents.Queries.GetMyResidentProfile;
using NestHub.Application.Vendors.Dtos;
using NestHub.Application.Vendors.Queries.GetMyFavoriteVendors;
using NestHub.Application.Vendors.Queries.GetMyMutedVendorIds;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/residents")]
[Authorize]
public sealed class ResidentsController : ControllerBase
{
    private readonly ISender _sender;

    public ResidentsController(ISender sender) => _sender = sender;

    [HttpGet("me")]
    [ProducesResponseType(typeof(ResidentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resident = await _sender.Send(new GetMyResidentProfileQuery(userId), cancellationToken);
        return resident is null ? NoContent() : Ok(resident);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ResidentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterResidentCommand command, CancellationToken cancellationToken)
    {
        var resident = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = resident.Id }, resident);
    }

    [HttpGet("me/favorites")]
    [ProducesResponseType(typeof(IReadOnlyList<VendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyFavorites(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resident = await _sender.Send(new GetMyResidentProfileQuery(userId), cancellationToken);
        if (resident is null) return Ok(Array.Empty<VendorDto>());

        var favorites = await _sender.Send(new GetMyFavoriteVendorsQuery(resident.Id), cancellationToken);
        return Ok(favorites);
    }

    [HttpGet("me/muted-vendors")]
    [ProducesResponseType(typeof(IReadOnlyList<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyMutedVendors(CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var resident = await _sender.Send(new GetMyResidentProfileQuery(userId), cancellationToken);
        if (resident is null) return Ok(Array.Empty<Guid>());

        var mutedVendorIds = await _sender.Send(new GetMyMutedVendorIdsQuery(resident.Id), cancellationToken);
        return Ok(mutedVendorIds);
    }
}
