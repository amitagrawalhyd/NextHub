using MediatR;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Vendors.Queries.SearchVendors;

public sealed record SearchVendorsQuery(string? Query, string? Category, Guid? ResidentSocietyId = null) : IRequest<IReadOnlyList<VendorDto>>;

public sealed class SearchVendorsQueryHandler : IRequestHandler<SearchVendorsQuery, IReadOnlyList<VendorDto>>
{
    private const double NearbyRadiusKm = 5.0;
    private const string TierInHouse = "InHouse";
    private const string TierNearby = "Nearby";
    private const string TierOther = "Other";

    private readonly IVendorRepository _vendorRepository;
    private readonly IAiService _aiService;
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly ISocietyRepository _societyRepository;

    public SearchVendorsQueryHandler(
        IVendorRepository vendorRepository,
        IAiService aiService,
        IVendorSocietyCoverageRepository coverageRepository,
        ISocietyRepository societyRepository)
    {
        _vendorRepository = vendorRepository;
        _aiService = aiService;
        _coverageRepository = coverageRepository;
        _societyRepository = societyRepository;
    }

    public async Task<IReadOnlyList<VendorDto>> Handle(SearchVendorsQuery request, CancellationToken cancellationToken)
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

        if (request.ResidentSocietyId is null || results.Count == 0)
            return results;

        return await ApplyTiersAsync(results, request.ResidentSocietyId.Value, cancellationToken);
    }

    /// <summary>
    /// Single pass: fetch all coverage rows and all society geo-locations once, then classify
    /// every result in memory. No per-vendor round trips.
    /// </summary>
    private async Task<IReadOnlyList<VendorDto>> ApplyTiersAsync(List<VendorDto> results, Guid residentSocietyId, CancellationToken cancellationToken)
    {
        var allCoverage = await _coverageRepository.GetAllAsync(cancellationToken);
        var societies = await _societyRepository.GetActiveAsync(cancellationToken);
        var societyLocationsById = societies.ToDictionary(s => s.Id.Value, s => s.GeoLocation);

        var coverageByVendor = allCoverage
            .GroupBy(c => c.VendorId.Value)
            .ToDictionary(g => g.Key, g => g.Select(c => c.SocietyId.Value).ToList());

        societyLocationsById.TryGetValue(residentSocietyId, out var residentLocation);

        return results
            .Select(dto => dto with { Tier = ClassifyTier(dto.Id, residentSocietyId, residentLocation, coverageByVendor, societyLocationsById) })
            .ToList();
    }

    private static string ClassifyTier(
        Guid vendorId,
        Guid residentSocietyId,
        GeoLocation? residentLocation,
        IReadOnlyDictionary<Guid, List<Guid>> coverageByVendor,
        IReadOnlyDictionary<Guid, GeoLocation?> societyLocationsById)
    {
        if (!coverageByVendor.TryGetValue(vendorId, out var coveredSocietyIds) || coveredSocietyIds.Count == 0)
            return TierOther;

        if (coveredSocietyIds.Contains(residentSocietyId))
            return TierInHouse;

        if (residentLocation is null)
            return TierOther;

        foreach (var societyId in coveredSocietyIds)
        {
            if (societyLocationsById.TryGetValue(societyId, out var location) && location is not null
                && HaversineDistanceKm(residentLocation, location) <= NearbyRadiusKm)
                return TierNearby;
        }

        return TierOther;
    }

    private static double HaversineDistanceKm(GeoLocation a, GeoLocation b)
    {
        const double earthRadiusKm = 6371.0;
        var dLat = ToRadians(b.Latitude - a.Latitude);
        var dLon = ToRadians(b.Longitude - a.Longitude);
        var lat1 = ToRadians(a.Latitude);
        var lat2 = ToRadians(b.Latitude);

        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));

        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
