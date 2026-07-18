using NestHub.Domain.Common;
using NestHub.Domain.Enums;

namespace NestHub.Domain.Analytics;

public sealed class AnalyticsLog : AggregateRoot<AnalyticsLogId>
{
    public VendorId VendorId { get; private set; }
    public AnalyticsActionType ActionType { get; private set; }
    public DateTime CreatedDateTimeUtc { get; private set; }

    private AnalyticsLog()
    {
    }

    private AnalyticsLog(AnalyticsLogId id, VendorId vendorId, AnalyticsActionType actionType)
    {
        Id = id;
        VendorId = vendorId;
        ActionType = actionType;
        CreatedDateTimeUtc = DateTime.UtcNow;
    }

    public static AnalyticsLog Record(VendorId vendorId, AnalyticsActionType actionType) =>
        new(AnalyticsLogId.New(), vendorId, actionType);
}
