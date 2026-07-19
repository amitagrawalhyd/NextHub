namespace NestHub.Application.Vendors.Common;

/// <summary>
/// Emoji + brand-palette hex color per category, used as a vendor's fallback "logo" whenever
/// they haven't uploaded one. Colors are drawn from the same semantic palette every client
/// already uses (Admin's _Layout.cshtml brand vars / Mobile's Colors.xaml) so the fallback icon
/// looks native wherever it's shown rather than introducing a new ad-hoc palette.
/// </summary>
public static class VendorCategoryIcon
{
    public static (string Emoji, string HexColor) For(string category) => category switch
    {
        "Plumbing" => ("🔧", "#2C7BB6"),
        "Electrical" => ("⚡", "#D69526"),
        "Pest Control" => ("🐛", "#2E9E5B"),
        "Tutors" => ("📚", "#1F5C99"),
        "Food & Catering" => ("🍽️", "#F2872E"),
        "Health & Wellness" => ("❤️", "#D64550"),
        "Appliance Repair" => ("🔌", "#123A61"),
        "Home Maintenance" => ("🔨", "#6E6E6E"),
        _ => ("🏢", "#919191"),
    };
}
