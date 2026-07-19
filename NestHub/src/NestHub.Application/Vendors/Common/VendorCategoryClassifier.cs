namespace NestHub.Application.Vendors.Common;

/// <summary>
/// Picks a single display category for a vendor when they have no services yet (e.g. still
/// pending approval) or as a stable fallback icon key. Prefers the vendor's own service
/// categories (an authoritative signal) and falls back to keyword-matching their business
/// name/bio against the platform's known category names.
/// </summary>
public static class VendorCategoryClassifier
{
    public const string DefaultCategory = "General Services";

    private static readonly (string Category, string[] Keywords)[] Rules =
    {
        ("Plumbing", new[] { "plumb", "pipe", "tap ", "leak", "drainage" }),
        ("Electrical", new[] { "electric", "wiring", "spark", "switch", "fuse" }),
        ("Pest Control", new[] { "pest", "termite", "cockroach", "rodent", "fumigation" }),
        ("Tutors", new[] { "tutor", "tuition", "coaching", "teach", "academy", "math" }),
        ("Food & Catering", new[] { "food", "caterer", "catering", "chef", "tiffin", "kitchen" }),
        ("Health & Wellness", new[] { "health", "wellness", "clinic", "doctor", "physio", "yoga", "fitness" }),
        ("Appliance Repair", new[] { "appliance", "air condition", " ac ", "fridge", "refrigerator", "washing machine", "mechanic" }),
        ("Home Maintenance", new[] { "carpenter", "carpentry", "handyman", "maintenance", "paint", "renovation", "furniture" }),
    };

    public static string Classify(string businessName, string? bio, IEnumerable<string>? serviceCategories = null)
    {
        var mostCommonServiceCategory = serviceCategories
            ?.Where(c => !string.IsNullOrWhiteSpace(c))
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        if (mostCommonServiceCategory is not null)
            return mostCommonServiceCategory;

        var haystack = $" {businessName} {bio} ".ToLowerInvariant();
        foreach (var (category, keywords) in Rules)
        {
            if (keywords.Any(haystack.Contains))
                return category;
        }

        return DefaultCategory;
    }
}
