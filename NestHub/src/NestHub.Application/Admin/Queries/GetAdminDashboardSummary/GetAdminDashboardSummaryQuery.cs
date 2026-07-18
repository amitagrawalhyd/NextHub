using MediatR;
using NestHub.Application.Vendors.Queries.GetMostFavoritedVendors;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Admin.Queries.GetAdminDashboardSummary;

public sealed record DailySosCountDto(DateOnly Date, int Count);

public sealed record AdminDashboardSummaryDto(
    bool IsSocietyScoped,
    string? SocietyName,
    int TotalSocieties,
    int TotalResidents,
    int ApprovedVendors,
    int PendingVendorApprovals,
    int FeaturedVendors,
    int OpenSosRequests,
    int FlaggedReviews,
    IReadOnlyList<MostFavoritedVendorDto> TopFavoritedVendors,
    IReadOnlyList<DailySosCountDto> SosTrend);

public sealed record GetAdminDashboardSummaryQuery(Guid? SocietyId = null) : IRequest<AdminDashboardSummaryDto>;

public sealed class GetAdminDashboardSummaryQueryHandler : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryDto>
{
    private const int SosTrendDays = 7;

    private readonly ISocietyRepository _societyRepository;
    private readonly IResidentRepository _residentRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly ISosRequestRepository _sosRequestRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ISender _sender;

    public GetAdminDashboardSummaryQueryHandler(
        ISocietyRepository societyRepository,
        IResidentRepository residentRepository,
        IVendorRepository vendorRepository,
        ISosRequestRepository sosRequestRepository,
        IReviewRepository reviewRepository,
        ISender sender)
    {
        _societyRepository = societyRepository;
        _residentRepository = residentRepository;
        _vendorRepository = vendorRepository;
        _sosRequestRepository = sosRequestRepository;
        _reviewRepository = reviewRepository;
        _sender = sender;
    }

    public async Task<AdminDashboardSummaryDto> Handle(GetAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var societies = await _societyRepository.GetActiveAsync(cancellationToken);
        var residents = await _residentRepository.GetAllAsync(cancellationToken);
        var flaggedReviews = await _reviewRepository.GetFlaggedAsync(cancellationToken);

        var toUtc = DateTime.UtcNow;
        var fromUtc = toUtc.AddDays(-(SosTrendDays - 1)).Date;
        var sosInRange = await _sosRequestRepository.GetAllInRangeAsync(fromUtc, toUtc, cancellationToken);

        if (request.SocietyId is { } societyGuid)
        {
            var societyId = new SocietyId(societyGuid);
            var society = societies.FirstOrDefault(s => s.Id == societyId);

            var scopedResidentCount = residents.Count(r => r.SocietyId == societyId);
            var scopedFlaggedReviews = flaggedReviews.Count(r => r.SocietyId == societyId);
            var scopedOpenSos = await _sosRequestRepository.CountOpenBySocietyAsync(societyId, cancellationToken);
            var scopedSosTrend = BuildTrend(sosInRange.Where(r => r.SocietyId == societyId), fromUtc);

            return new AdminDashboardSummaryDto(
                IsSocietyScoped: true,
                SocietyName: society?.Name,
                TotalSocieties: 1,
                TotalResidents: scopedResidentCount,
                ApprovedVendors: 0,
                PendingVendorApprovals: 0,
                FeaturedVendors: 0,
                OpenSosRequests: scopedOpenSos,
                FlaggedReviews: scopedFlaggedReviews,
                TopFavoritedVendors: Array.Empty<MostFavoritedVendorDto>(),
                SosTrend: scopedSosTrend);
        }

        var approvedVendors = await _vendorRepository.GetAllApprovedAsync(cancellationToken);
        var pendingVendors = await _vendorRepository.GetPendingApprovalAsync(cancellationToken);
        var openSosCount = await _sosRequestRepository.CountOpenAsync(cancellationToken);
        var topFavorited = await _sender.Send(new GetMostFavoritedVendorsQuery(5), cancellationToken);
        var sosTrend = BuildTrend(sosInRange, fromUtc);

        return new AdminDashboardSummaryDto(
            IsSocietyScoped: false,
            SocietyName: null,
            TotalSocieties: societies.Count,
            TotalResidents: residents.Count,
            ApprovedVendors: approvedVendors.Count,
            PendingVendorApprovals: pendingVendors.Count,
            FeaturedVendors: approvedVendors.Count(v => v.IsFeatured),
            OpenSosRequests: openSosCount,
            FlaggedReviews: flaggedReviews.Count,
            TopFavoritedVendors: topFavorited,
            SosTrend: sosTrend);
    }

    /// <summary>Single pass over the already-fetched range to bucket counts per day.</summary>
    private static IReadOnlyList<DailySosCountDto> BuildTrend(IEnumerable<Domain.SosRequests.SosRequest> requests, DateTime fromUtc)
    {
        var countsByDate = requests
            .GroupBy(r => DateOnly.FromDateTime(r.CreatedDateTimeUtc))
            .ToDictionary(g => g.Key, g => g.Count());

        var trend = new List<DailySosCountDto>();
        for (var date = DateOnly.FromDateTime(fromUtc); date <= DateOnly.FromDateTime(DateTime.UtcNow); date = date.AddDays(1))
            trend.Add(new DailySosCountDto(date, countsByDate.TryGetValue(date, out var count) ? count : 0));

        return trend;
    }
}
