using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Exceptions;

namespace NestHub.Domain.SosRequests;

public sealed class SosRequest : AggregateRoot<SosRequestId>
{
    private readonly List<SosClaim> _claims = new();

    public ResidentId ResidentId { get; private set; }
    public SocietyId SocietyId { get; private set; }
    public string Category { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public SosStatus Status { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }
    public IReadOnlyCollection<SosClaim> Claims => _claims.AsReadOnly();

    private SosRequest()
    {
    }

    private SosRequest(SosRequestId id, ResidentId residentId, SocietyId societyId, string category, string description)
    {
        Id = id;
        ResidentId = residentId;
        SocietyId = societyId;
        Category = category;
        Description = description;
        Status = SosStatus.Open;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static SosRequest RaiseNew(ResidentId residentId, SocietyId societyId, string category, string description)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required.", nameof(category));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        var request = new SosRequest(SosRequestId.New(), residentId, societyId, category.Trim(), description.Trim());
        request.RaiseDomainEvent(new SosRequestCreatedDomainEvent(request.Id, societyId, request.Category));
        return request;
    }

    public SosClaim ClaimBy(VendorId vendorId)
    {
        if (Status == SosStatus.Closed)
            throw new SosRequestClosedException(Id);
        if (_claims.Any(c => c.VendorId == vendorId))
            throw new SosRequestAlreadyClaimedByVendorException(Id, vendorId);

        var claim = SosClaim.Create(Id, vendorId);
        _claims.Add(claim);

        if (Status == SosStatus.Open)
            Status = SosStatus.Claimed;

        RaiseDomainEvent(new SosRequestClaimedDomainEvent(Id, vendorId));
        return claim;
    }

    public void Close()
    {
        if (Status == SosStatus.Closed)
            throw new SosRequestClosedException(Id);

        Status = SosStatus.Closed;
        RaiseDomainEvent(new SosRequestClosedDomainEvent(Id));
    }
}
