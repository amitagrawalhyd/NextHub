using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Common.Models;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.SearchVendors;

public sealed record SearchVendorsQuery(
    string? Query,
    string? Category,
    Guid? ResidentSocietyId = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<VendorDto>>;

public sealed class SearchVendorsQueryHandler : IRequestHandler<SearchVendorsQuery, PagedResult<VendorDto>>
{
    private const string TierInHouse = "InHouse";
    private const string TierNearby = "Nearby";
    private const string TierOther = "Other";

    private readonly IVendorRepository _vendorRepository;
    private readonly IAiService _aiService;
    private readonly IVendorSocietyCoverageRepository _coverageRepository;

    public SearchVendorsQueryHandler(
        IVendorRepository vendorRepository,
        IAiService aiService,
        IVendorSocietyCoverageRepository coverageRepository)
    {
        _vendorRepository = vendorRepository;
        _aiService = aiService;
        _coverageRepository = coverageRepository;
    }

    public async Task<PagedResult<VendorDto>> Handle(SearchVendorsQuery request, CancellationToken cancellationToken)
    {
        var candidates = string.IsNullOrWhiteSpace(request.Category)
            ? await _vendorRepository.GetAllApprovedAsync(cancellationToken)
            : await _vendorRepository.GetByCategoryAsync(request.Category, cancellationToken);

        List<VendorDto> results;
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            results = candidates
                .OrderByDescending(v => v.IsFeatured)
                .ThenByDescending(v => v.AverageRating)
                .Select(v => v.ToDto())
                .ToList();
        }
        else
        {
            var searchable = candidates
                .Select(v => new SearchableVendor(
                    v.Id.Value,
                    v.BusinessName,
                    v.Bio ?? string.Empty,
                    v.Services.Select(s => s.Title).ToList(),
                    v.Services.Select(s => s.Category).Distinct().ToList()))
                .ToList();

            var rankedIds = _aiService.RankVendorsBySearchRelevance(request.Query, searchable);
            var vendorsById = candidates.ToDictionary(v => v.Id, v => v);

            results = rankedIds
                .Select(id => new VendorId(id))
                .Where(vendorsById.ContainsKey)
                .Select(id => vendorsById[id].ToDto())
                .ToList();
        }

        if (request.ResidentSocietyId is { } residentSocietyId && results.Count > 0)
            results = await ApplyTiersAndSortAsync(results, residentSocietyId, cancellationToken);

        var totalCount = results.Count;
        var page = results
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<VendorDto>(page, request.PageNumber, request.PageSize, totalCount);
    }

    /// <summary>
    /// Tiers come from a single indexed lookup — VendorSocietyCoverages rows for the resident's
    /// own society, already maintained event-driven (InHouse explicitly, Nearby automatically
    /// via RecomputeVendorProximityCommand) rather than recomputed with Haversine on every
    /// search. Manual rows (the pre-existing self-service coverage feature) tier as InHouse too,
    /// preserving that feature's original "any coverage row ⇒ InHouse" behavior.
    /// </summary>
    private async Task<List<VendorDto>> ApplyTiersAndSortAsync(List<VendorDto> results, Guid residentSocietyId, CancellationToken cancellationToken)
    {
        var coverageForSociety = await _coverageRepository.GetAllForSocietyAsync(new SocietyId(residentSocietyId), cancellationToken);
        var tierByVendorId = coverageForSociety.ToDictionary(
            c => c.VendorId.Value,
            c => c.AffiliationType == AffiliationType.Nearby ? TierNearby : TierInHouse);

        return results
            .Select(dto => dto with { Tier = tierByVendorId.GetValueOrDefault(dto.Id, TierOther) })
            .OrderBy(dto => TierOrdinal(dto.Tier))
            .ToList();
    }

    private static int TierOrdinal(string tier) => tier switch
    {
        TierInHouse => 0,
        TierNearby => 1,
        _ => 2
    };
}
