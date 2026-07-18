using FluentValidation;
using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string PhoneNumber, string? Email, string Password, UserType UserType) : IRequest<Guid>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.UserType).IsInEnum();
    }
}

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);

        if (await _userRepository.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
            throw new InvalidOperationException($"A user with phone number '{phoneNumber}' is already registered.");

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email);
        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Register(phoneNumber, email, passwordHash, request.UserType);
        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id.Value;
    }
}
