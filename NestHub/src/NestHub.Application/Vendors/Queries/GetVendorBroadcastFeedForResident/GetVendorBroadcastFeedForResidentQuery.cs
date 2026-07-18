using MediatR;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetVendorBroadcastFeedForResident;

public sealed record GetVendorBroadcastFeedForResidentQuery(Guid SocietyId, Guid? ResidentId = null) : IRequest<IReadOnlyList<VendorBroadcastDto>>;

public sealed class GetVendorBroadcastFeedForResidentQueryHandler : IRequestHandler<GetVendorBroadcastFeedForResidentQuery, IReadOnlyList<VendorBroadcastDto>>
{
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly IVendorBroadcastRepository _broadcastRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorMuteRepository _muteRepository;

    public GetVendorBroadcastFeedForResidentQueryHandler(
        IVendorSocietyCoverageRepository coverageRepository,
        IVendorBroadcastRepository broadcastRepository,
        IVendorRepository vendorRepository,
        IVendorMuteRepository muteRepository)
    {
        _coverageRepository = coverageRepository;
        _broadcastRepository = broadcastRepository;
        _vendorRepository = vendorRepository;
        _muteRepository = muteRepository;
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> Handle(GetVendorBroadcastFeedForResidentQuery request, CancellationToken cancellationToken)
    {
        var societyId = new SocietyId(request.SocietyId);

        // Single pass: fetch every vendor covering this society once, then fetch their active
        // broadcasts once, then batch-load vendor names once — no per-broadcast round trips.
        var coveringVendorIds = (await _coverageRepository.GetAllForSocietyAsync(societyId, cancellationToken))
            .Select(c => c.VendorId)
            .Distinct()
            .ToList();

        if (coveringVendorIds.Count == 0)
            return Array.Empty<VendorBroadcastDto>();

        if (request.ResidentId is { } residentGuid)
        {
            var mutedVendorIds = (await _muteRepository.GetByResidentIdAsync(new ResidentId(residentGuid), cancellationToken))
                .Select(m => m.VendorId)
                .ToHashSet();

            if (mutedVendorIds.Count > 0)
                coveringVendorIds = coveringVendorIds.Where(id => !mutedVendorIds.Contains(id)).ToList();

            if (coveringVendorIds.Count == 0)
                return Array.Empty<VendorBroadcastDto>();
        }

        var broadcasts = await _broadcastRepository.GetActiveByVendorIdsAsync(coveringVendorIds, cancellationToken);
        if (broadcasts.Count == 0)
            return Array.Empty<VendorBroadcastDto>();

        var vendors = await _vendorRepository.GetByIdsAsync(coveringVendorIds, cancellationToken);
        var vendorNames = vendors.ToDictionary(v => v.Id, v => v.BusinessName);

        return broadcasts
            .Select(b => new VendorBroadcastDto(
                b.Id.Value,
                b.VendorId.Value,
                vendorNames.TryGetValue(b.VendorId, out var name) ? name : "(vendor)",
                b.Title,
                b.Message,
                b.CreatedDateTimeUtc,
                b.ExpiresAtUtc))
            .ToList();
    }
}
