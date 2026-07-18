using NestHub.Application.Common.Interfaces;

namespace NestHub.Infrastructure.Services.Ai;

/// <summary>
/// Deterministic, dependency-free stand-in for the smart-search and sentiment-profiling
/// features: keyword-weighted relevance ranking and a positive/negative lexicon scorer.
/// Both are exposed only through <see cref="IAiService"/>, so a real OpenAI/Semantic Kernel
/// implementation can be swapped in later without touching any caller.
/// </summary>
public sealed class LocalAiService : IAiService
{
    private static readonly HashSet<string> PositiveWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "great", "excellent", "good", "amazing", "reliable", "friendly", "prompt", "professional",
        "clean", "affordable", "recommend", "helpful", "polite", "quick", "trustworthy", "best",
        "happy", "satisfied", "wonderful", "fantastic", "outstanding", "courteous", "responsive"
    };

    private static readonly HashSet<string> NegativeWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "bad", "poor", "late", "rude", "unprofessional", "dirty", "expensive", "disappointing",
        "terrible", "worst", "avoid", "delay", "delayed", "damaged", "unreliable", "careless",
        "overcharged", "unresponsive", "horrible", "awful", "sloppy", "cheat", "cheated"
    };

    public IReadOnlyList<Guid> RankVendorsBySearchRelevance(string query, IReadOnlyList<SearchableVendor> candidates)
    {
        var queryTerms = Tokenize(query);
        if (queryTerms.Count == 0)
            return candidates.Select(c => c.VendorId).ToList();

        return candidates
            .Select(candidate => (candidate.VendorId, Score: ScoreCandidate(queryTerms, candidate)))
            .Where(result => result.Score > 0)
            .OrderByDescending(result => result.Score)
            .Select(result => result.VendorId)
            .ToList();
    }

    public double ScoreSentiment(string text)
    {
        var words = Tokenize(text);
        if (words.Count == 0)
            return 0;

        var positiveHits = words.Count(w => PositiveWords.Contains(w));
        var negativeHits = words.Count(w => NegativeWords.Contains(w));
        var totalHits = positiveHits + negativeHits;

        if (totalHits == 0)
            return 0;

        var score = (double)(positiveHits - negativeHits) / totalHits;
        return Math.Clamp(score, -1, 1);
    }

    private static double ScoreCandidate(IReadOnlyList<string> queryTerms, SearchableVendor candidate)
    {
        double score = 0;

        foreach (var term in queryTerms)
        {
            if (candidate.BusinessName.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 3;

            if (candidate.Categories.Any(c => c.Contains(term, StringComparison.OrdinalIgnoreCase)))
                score += 2;

            if (candidate.ServiceTitles.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase)))
                score += 2;

            if (candidate.Bio.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 1;
        }

        return score;
    }

    private static IReadOnlyList<string> Tokenize(string text) =>
        string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            : text.Split(new[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
}
