using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Societies.Commands.RegisterSociety;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/societies")]
public sealed class SocietiesController : ControllerBase
{
    private readonly ISender _sender;

    public SocietiesController(ISender sender) => _sender = sender;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SocietyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var societies = await _sender.Send(new GetActiveSocietiesQuery(), cancellationToken);
        return Ok(societies);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SocietyDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterSocietyCommand command, CancellationToken cancellationToken)
    {
        var society = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetActive), society);
    }
}
