using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public Email? Email { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public UserType UserType { get; private set; }
    public bool IsVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private User()
    {
    }

    private User(UserId id, PhoneNumber phoneNumber, Email? email, string passwordHash, UserType userType)
    {
        Id = id;
        PhoneNumber = phoneNumber;
        Email = email;
        PasswordHash = passwordHash;
        UserType = userType;
        IsVerified = false;
        IsActive = true;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static User Register(PhoneNumber phoneNumber, Email? email, string passwordHash, UserType userType)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User(UserId.New(), phoneNumber, email, passwordHash, userType);
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, userType));
        return user;
    }

    public void MarkVerified()
    {
        if (IsVerified) return;
        IsVerified = true;
        RaiseDomainEvent(new UserVerifiedDomainEvent(Id));
    }

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));
        PasswordHash = newPasswordHash;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
