using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Announcements.Dtos;
using NestHub.Application.Announcements.Queries.GetActiveAnnouncementsForSociety;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize]
public sealed class AnnouncementsController : ControllerBase
{
    private readonly ISender _sender;

    public AnnouncementsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AnnouncementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive([FromQuery] Guid societyId, CancellationToken cancellationToken)
    {
        var announcements = await _sender.Send(new GetActiveAnnouncementsForSocietyQuery(societyId), cancellationToken);
        return Ok(announcements);
    }
}
