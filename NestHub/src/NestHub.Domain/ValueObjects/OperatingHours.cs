namespace NestHub.Domain.ValueObjects;

public sealed record DailyHours(TimeOnly? OpensAt, TimeOnly? ClosesAt, bool IsClosed)
{
    public static DailyHours Closed() => new(null, null, true);

    public static DailyHours Open(TimeOnly opensAt, TimeOnly closesAt)
    {
        if (closesAt <= opensAt)
            throw new ArgumentException("Closing time must be after opening time.", nameof(closesAt));
        return new DailyHours(opensAt, closesAt, false);
    }
}

public sealed record OperatingHours
{
    public IReadOnlyDictionary<DayOfWeek, DailyHours> Days { get; }

    private OperatingHours(IReadOnlyDictionary<DayOfWeek, DailyHours> days) => Days = days;

    public static OperatingHours Create(IReadOnlyDictionary<DayOfWeek, DailyHours> days)
    {
        var allDays = Enum.GetValues<DayOfWeek>();
        if (days.Count != allDays.Length || allDays.Any(d => !days.ContainsKey(d)))
            throw new ArgumentException("Operating hours must define an entry for every day of the week.", nameof(days));

        return new OperatingHours(days);
    }

    public static OperatingHours AlwaysOpen() =>
        new(Enum.GetValues<DayOfWeek>().ToDictionary(d => d, _ => DailyHours.Open(TimeOnly.MinValue, TimeOnly.MaxValue)));
}
