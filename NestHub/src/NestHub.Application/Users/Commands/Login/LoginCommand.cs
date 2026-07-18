using FluentValidation;
using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Users.Dtos;
using NestHub.Domain.Repositories;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Users.Commands.Login;

public sealed record LoginCommand(string PhoneNumber, string Password) : IRequest<AuthResultDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid phone number or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("This account has been deactivated.");

        var token = _jwtTokenGenerator.Generate(user.Id, user.UserType);
        return new AuthResultDto(user.Id.Value, user.UserType.ToString(), token.Token, token.ExpiresAtUtc);
    }
}
