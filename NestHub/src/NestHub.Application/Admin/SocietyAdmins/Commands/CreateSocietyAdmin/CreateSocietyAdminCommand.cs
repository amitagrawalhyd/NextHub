using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Interfaces;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;
using NestHub.Domain.Users;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Admin.SocietyAdmins.Commands.CreateSocietyAdmin;

public sealed record CreateSocietyAdminCommand(string PhoneNumber, string? Email, string Password, Guid SocietyId) : IRequest<Guid>;

public sealed class CreateSocietyAdminCommandValidator : AbstractValidator<CreateSocietyAdminCommand>
{
    public CreateSocietyAdminCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.SocietyId).NotEmpty();
    }
}

public sealed class CreateSocietyAdminCommandHandler : IRequestHandler<CreateSocietyAdminCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSocietyAdminCommandHandler(
        IUserRepository userRepository,
        ISocietyRepository societyRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _societyRepository = societyRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSocietyAdminCommand request, CancellationToken cancellationToken)
    {
        var societyId = new SocietyId(request.SocietyId);
        _ = await _societyRepository.GetByIdAsync(societyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
        if (await _userRepository.ExistsByPhoneNumberAsync(phoneNumber, cancellationToken))
            throw new InvalidOperationException($"A user with phone number '{phoneNumber}' is already registered.");

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : Email.Create(request.Email);
        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Register(phoneNumber, email, passwordHash, UserType.Admin, societyId);
        user.MarkVerified();

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id.Value;
    }
}
