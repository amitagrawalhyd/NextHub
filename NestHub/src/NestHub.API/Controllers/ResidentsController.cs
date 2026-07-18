using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Residents.Commands.RegisterResident;
using NestHub.Application.Residents.Dtos;
using NestHub.Application.Residents.Queries.GetMyResidentProfile;

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
}
