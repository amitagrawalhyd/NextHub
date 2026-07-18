using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.EmergencyContacts.Dtos;
using NestHub.Application.EmergencyContacts.Queries.GetEmergencyContactsForSociety;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/emergency-contacts")]
[Authorize]
public sealed class EmergencyContactsController : ControllerBase
{
    private readonly ISender _sender;

    public EmergencyContactsController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EmergencyContactDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForSociety([FromQuery] Guid societyId, CancellationToken cancellationToken)
    {
        var contacts = await _sender.Send(new GetEmergencyContactsForSocietyQuery(societyId), cancellationToken);
        return Ok(contacts);
    }
}
