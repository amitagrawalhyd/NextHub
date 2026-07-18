using MediatR;
using Microsoft.AspNetCore.Mvc;
using NestHub.Admin.Common;
using NestHub.Application.Users.Commands.ResetPassword;
using NestHub.Application.Users.Commands.SetUserActive;
using NestHub.Application.Users.Commands.VerifyUser;
using NestHub.Application.Users.Dtos;
using NestHub.Application.Users.Queries.GetAllUsers;

namespace NestHub.Admin.Pages.Users;

public sealed class IndexModel : CentralAdminOnlyPageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<AdminUserDto> Users { get; private set; } = Array.Empty<AdminUserDto>();

    public async Task OnGetAsync()
    {
        Users = await _sender.Send(new GetAllUsersQuery());
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid userId, string newPassword)
    {
        await _sender.Send(new ResetPasswordCommand(userId, newPassword));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetActiveAsync(Guid userId, bool isActive)
    {
        await _sender.Send(new SetUserActiveCommand(userId, isActive));
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostVerifyAsync(Guid userId)
    {
        await _sender.Send(new VerifyUserCommand(userId));
        return RedirectToPage();
    }
}
