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

    /// <summary>
    /// Non-null only for Admin users scoped to manage a single society; null means a Central
    /// Admin with unrestricted access across every society.
    /// </summary>
    public SocietyId? SocietyId { get; private set; }

    private User()
    {
    }

    private User(UserId id, PhoneNumber phoneNumber, Email? email, string passwordHash, UserType userType, SocietyId? societyId)
    {
        Id = id;
        PhoneNumber = phoneNumber;
        Email = email;
        PasswordHash = passwordHash;
        UserType = userType;
        IsVerified = false;
        IsActive = true;
        CreatedDateTimeUtc = DateTime.UtcNow;
        SocietyId = societyId;
    }

    public static User Register(PhoneNumber phoneNumber, Email? email, string passwordHash, UserType userType, SocietyId? societyId = null)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        var user = new User(UserId.New(), phoneNumber, email, passwordHash, userType, societyId);
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
