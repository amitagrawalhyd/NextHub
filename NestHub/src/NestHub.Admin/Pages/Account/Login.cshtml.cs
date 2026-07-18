using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Admin.Common;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;

namespace NestHub.Admin.Pages.Account;

public sealed class LoginModel : PageModel
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginModel(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var phoneNumber = PhoneNumber.Create(Input.PhoneNumber);
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

        if (user is null || user.UserType != UserType.Admin || !_passwordHasher.Verify(Input.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid phone number or password, or this account is not an Admin account.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new(ClaimTypes.Role, UserType.Admin.ToString()),
            new(ClaimTypes.MobilePhone, phoneNumber.Value)
        };

        if (user.SocietyId is { } societyId)
            claims.Add(ClaimsPrincipalExtensions.SocietyIdClaim(societyId.Value));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return LocalRedirect(returnUrl ?? "/Vendors/Index");
    }

    public sealed class LoginInput
    {
        [Required]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
