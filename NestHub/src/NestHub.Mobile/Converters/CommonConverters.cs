using System.Globalization;
using NestHub.Application.Vendors.Common;
using NestHub.Mobile.Resources.Strings;

namespace NestHub.Mobile.Converters;

public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value is bool b && !b;
}

public sealed class StringNotEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => !string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class StringIsEmptyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => string.IsNullOrWhiteSpace(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Maps a vendor Tier code ("All"/"InHouse"/"Nearby"/"Other") to its resident-facing,
/// localized label. Re-evaluates on every binding refresh (e.g. a fresh search), so a language
/// switch takes effect on the next reload rather than instantaneously on an already-rendered list.</summary>
public sealed class TierToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var strings = LocalizationResourceManager.Instance;
        return (value as string) switch
        {
            "All" => strings["TierAllLabel"],
            "InHouse" => strings["TierInHouseLabel"],
            "Nearby" => strings["TierNearbyLabel"],
            _ => strings["TierOtherLabel"]
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class TrustBadgeFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["TrustBadgeFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class AverageRatingFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["AverageRatingFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class SubscriptionFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["SubscriptionFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class ApprovalStatusFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["ApprovalStatusFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class PostedFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["PostedFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class ReportedFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Format(LocalizationResourceManager.Instance["ReportedFormat"], value);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class CountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is int count && count > 0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class TierToIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value as string) switch
    {
        "InHouse" => "icon_tier_inhouse",
        "Nearby" => "icon_tier_nearby",
        _ => "icon_tier_other"
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class TierToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => (value as string) switch
    {
        "InHouse" => Color.FromArgb("#2E9E5B"),
        "Nearby" => Color.FromArgb("#2C7BB6"),
        _ => Color.FromArgb("#919191")
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>Vendor logo fallback: emoji per category, shown in a colored circle when a vendor
/// has no LogoUrl. Reuses NestHub.Application's VendorCategoryIcon so Admin (web) and Mobile
/// never disagree on which icon/color represents a category.</summary>
public sealed class CategoryToEmojiConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        VendorCategoryIcon.For(value as string ?? string.Empty).Emoji;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

public sealed class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        Color.FromArgb(VendorCategoryIcon.For(value as string ?? string.Empty).HexColor);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
