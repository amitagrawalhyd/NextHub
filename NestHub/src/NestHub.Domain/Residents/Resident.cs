using NestHub.Domain.Common;

namespace NestHub.Domain.Residents;

public sealed class Resident : AggregateRoot<ResidentId>
{
    public UserId UserId { get; private set; }
    public SocietyId SocietyId { get; private set; }
    public string BlockNumber { get; private set; } = null!;
    public string FlatNumber { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private Resident()
    {
    }

    private Resident(ResidentId id, UserId userId, SocietyId societyId, string blockNumber, string flatNumber, string name)
    {
        Id = id;
        UserId = userId;
        SocietyId = societyId;
        BlockNumber = blockNumber;
        FlatNumber = flatNumber;
        Name = name;
    }

    public static Resident Create(UserId userId, SocietyId societyId, string blockNumber, string flatNumber, string name)
    {
        if (string.IsNullOrWhiteSpace(blockNumber))
            throw new ArgumentException("Block number is required.", nameof(blockNumber));
        if (string.IsNullOrWhiteSpace(flatNumber))
            throw new ArgumentException("Flat number is required.", nameof(flatNumber));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        var resident = new Resident(ResidentId.New(), userId, societyId, blockNumber.Trim(), flatNumber.Trim(), name.Trim());
        resident.RaiseDomainEvent(new ResidentJoinedSocietyDomainEvent(resident.Id, societyId));
        return resident;
    }

    public void UpdateFlatDetails(string blockNumber, string flatNumber)
    {
        if (string.IsNullOrWhiteSpace(blockNumber))
            throw new ArgumentException("Block number is required.", nameof(blockNumber));
        if (string.IsNullOrWhiteSpace(flatNumber))
            throw new ArgumentException("Flat number is required.", nameof(flatNumber));

        BlockNumber = blockNumber.Trim();
        FlatNumber = flatNumber.Trim();
    }
}
