namespace NestHub.Application.Common.Interfaces;

public sealed record SearchableVendor(Guid VendorId, string BusinessName, string Bio, IReadOnlyList<string> ServiceTitles, IReadOnlyList<string> Categories);

public interface IAiService
{
    IReadOnlyList<Guid> RankVendorsBySearchRelevance(string query, IReadOnlyList<SearchableVendor> candidates);

    double ScoreSentiment(string text);
}
