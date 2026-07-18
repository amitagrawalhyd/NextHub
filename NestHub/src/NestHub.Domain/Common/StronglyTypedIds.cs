namespace NestHub.Domain.Common;

public readonly record struct SocietyId(Guid Value)
{
    public static SocietyId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct ResidentId(Guid Value)
{
    public static ResidentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct VendorId(Guid Value)
{
    public static VendorId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct ServiceId(Guid Value)
{
    public static ServiceId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct ReviewId(Guid Value)
{
    public static ReviewId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct SosRequestId(Guid Value)
{
    public static SosRequestId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct SosClaimId(Guid Value)
{
    public static SosClaimId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct AnalyticsLogId(Guid Value)
{
    public static AnalyticsLogId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}

public readonly record struct CategoryId(Guid Value)
{
    public static CategoryId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
