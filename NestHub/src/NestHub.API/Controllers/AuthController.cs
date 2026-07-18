using MediatR;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Users.Commands.Login;
using NestHub.Application.Users.Commands.RegisterUser;
using NestHub.Application.Users.Commands.VerifyUser;
using NestHub.Application.Users.Dtos;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("register")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Register), new { id = userId }, userId);
    }

    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Verify(VerifyUserCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
