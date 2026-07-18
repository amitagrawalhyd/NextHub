using FluentValidation;
using MediatR;
using NestHub.Application.Analytics.Dtos;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Analytics.Queries.GetTopVendorsByEngagement;

public sealed record GetTopVendorsByEngagementQuery(DateTime FromUtc, DateTime ToUtc, int Take = 5) : IRequest<IReadOnlyList<TopVendorDto>>;

public sealed class GetTopVendorsByEngagementQueryValidator : AbstractValidator<GetTopVendorsByEngagementQuery>
{
    public GetTopVendorsByEngagementQueryValidator()
    {
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc);
        RuleFor(x => x.Take).GreaterThan(0);
    }
}

public sealed class GetTopVendorsByEngagementQueryHandler : IRequestHandler<GetTopVendorsByEngagementQuery, IReadOnlyList<TopVendorDto>>
{
    private readonly IAnalyticsLogRepository _analyticsLogRepository;
    private readonly IVendorRepository _vendorRepository;

    public GetTopVendorsByEngagementQueryHandler(IAnalyticsLogRepository analyticsLogRepository, IVendorRepository vendorRepository)
    {
        _analyticsLogRepository = analyticsLogRepository;
        _vendorRepository = vendorRepository;
    }

    /// <summary>
    /// Single pass: fetch every log in range once, aggregate per vendor in memory, then batch-load
    /// only the top vendors' names in one call. No per-vendor round trips.
    /// </summary>
    public async Task<IReadOnlyList<TopVendorDto>> Handle(GetTopVendorsByEngagementQuery request, CancellationToken cancellationToken)
    {
        var logs = await _analyticsLogRepository.GetAllInRangeAsync(request.FromUtc, request.ToUtc, cancellationToken);

        var ranked = logs
            .GroupBy(l => l.VendorId)
            .Select(g => new
            {
                VendorId = g.Key,
                ProfileViews = g.Count(l => l.ActionType == AnalyticsActionType.ProfileView),
                CallClicks = g.Count(l => l.ActionType == AnalyticsActionType.CallClick),
                WhatsAppClicks = g.Count(l => l.ActionType == AnalyticsActionType.WhatsAppClick),
                TotalEngagements = g.Count()
            })
            .OrderByDescending(x => x.TotalEngagements)
            .Take(request.Take)
            .ToList();

        if (ranked.Count == 0)
            return Array.Empty<TopVendorDto>();

        var vendors = await _vendorRepository.GetByIdsAsync(ranked.Select(r => r.VendorId), cancellationToken);
        var businessNameById = vendors.ToDictionary(v => v.Id, v => v.BusinessName);

        return ranked
            .Select(r => new TopVendorDto(
                r.VendorId.Value,
                businessNameById.TryGetValue(r.VendorId, out var name) ? name : "Unknown Vendor",
                r.ProfileViews,
                r.CallClicks,
                r.WhatsAppClicks,
                r.TotalEngagements))
            .ToList();
    }
}
