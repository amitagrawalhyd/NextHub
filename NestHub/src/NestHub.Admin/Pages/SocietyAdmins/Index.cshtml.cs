using MediatR;
using Microsoft.AspNetCore.Mvc;
using NestHub.Admin.Common;
using NestHub.Application.Admin.SocietyAdmins.Commands.CreateSocietyAdmin;
using NestHub.Application.Admin.SocietyAdmins.Commands.ReassignSocietyAdmin;
using NestHub.Application.Admin.SocietyAdmins.Dtos;
using NestHub.Application.Admin.SocietyAdmins.Queries.GetSocietyAdmins;
using NestHub.Application.Societies.Dtos;
using NestHub.Application.Societies.Queries.GetActiveSocieties;
using NestHub.Application.Users.Commands.ResetPassword;
using NestHub.Application.Users.Commands.SetUserActive;

namespace NestHub.Admin.Pages.SocietyAdmins;

public sealed class IndexModel : CentralAdminOnlyPageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<SocietyAdminDto> SocietyAdmins { get; private set; } = Array.Empty<SocietyAdminDto>();
    public IReadOnlyList<SocietyDto> AllSocieties { get; private set; } = Array.Empty<SocietyDto>();

    public async Task OnGetAsync()
    {
        SocietyAdmins = await _sender.Send(new GetSocietyAdminsQuery());
        AllSocieties = await _sender.Send(new GetActiveSocietiesQuery());
    }

    public async Task<IActionResult> OnPostCreateAsync(string phoneNumber, string? email, string password, Guid societyId)
    {
        if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(password) && societyId != Guid.Empty)
            await _sender.Send(new CreateSocietyAdminCommand(phoneNumber, email, password, societyId));

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReassignAsync(Guid userId, Guid societyId)
    {
        await _sender.Send(new ReassignSocietyAdminCommand(userId, societyId));
        return RedirectToPage();
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
}
