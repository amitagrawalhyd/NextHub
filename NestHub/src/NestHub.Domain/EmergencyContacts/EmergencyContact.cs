using NestHub.Domain.Common;

namespace NestHub.Domain.EmergencyContacts;

public sealed class EmergencyContact : Entity<EmergencyContactId>
{
    public SocietyId SocietyId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;

    private EmergencyContact()
    {
    }

    private EmergencyContact(EmergencyContactId id, SocietyId societyId, string name, string role, string phoneNumber)
    {
        Id = id;
        SocietyId = societyId;
        Name = name;
        Role = role;
        PhoneNumber = phoneNumber;
    }

    public static EmergencyContact Create(SocietyId societyId, string name, string role, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        return new EmergencyContact(EmergencyContactId.New(), societyId, name.Trim(), role?.Trim() ?? string.Empty, phoneNumber.Trim());
    }
}
