using NestHub.Domain.Common;
using NestHub.Domain.ValueObjects;

namespace NestHub.Domain.Societies;

public sealed class Society : AggregateRoot<SocietyId>
{
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public GeoLocation? GeoLocation { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private Society()
    {
    }

    private Society(SocietyId id, string name, string address, string city, GeoLocation? geoLocation)
    {
        Id = id;
        Name = name;
        Address = address;
        City = city;
        GeoLocation = geoLocation;
        IsActive = true;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static Society Register(string name, string address, GeoLocation? geoLocation, string city = "Hyderabad")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Society name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Society address is required.", nameof(address));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required.", nameof(city));

        var society = new Society(SocietyId.New(), name.Trim(), address.Trim(), city.Trim(), geoLocation);
        society.RaiseDomainEvent(new SocietyRegisteredDomainEvent(society.Id));
        return society;
    }

    public void UpdateDetails(string name, string address, GeoLocation? geoLocation)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Society name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Society address is required.", nameof(address));

        Name = name.Trim();
        Address = address.Trim();
        GeoLocation = geoLocation;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
