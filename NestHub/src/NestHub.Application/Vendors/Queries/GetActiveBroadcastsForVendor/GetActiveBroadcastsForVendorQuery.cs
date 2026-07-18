using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Queries.GetActiveBroadcastsForVendor;

/// <summary>
/// A single vendor's currently-active broadcasts, shown on that vendor's own profile page —
/// always visible here regardless of a resident's mute preference, since viewing the profile is
/// a deliberate look-up. Muting only affects the aggregated Home feed.
/// </summary>
public sealed record GetActiveBroadcastsForVendorQuery(Guid VendorId) : IRequest<IReadOnlyList<VendorBroadcastDto>>;

public sealed class GetActiveBroadcastsForVendorQueryHandler : IRequestHandler<GetActiveBroadcastsForVendorQuery, IReadOnlyList<VendorBroadcastDto>>
{
    private readonly IVendorBroadcastRepository _broadcastRepository;
    private readonly IVendorRepository _vendorRepository;

    public GetActiveBroadcastsForVendorQueryHandler(IVendorBroadcastRepository broadcastRepository, IVendorRepository vendorRepository)
    {
        _broadcastRepository = broadcastRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> Handle(GetActiveBroadcastsForVendorQuery request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var broadcasts = await _broadcastRepository.GetActiveByVendorIdsAsync(new[] { vendorId }, cancellationToken);

        return broadcasts
            .Select(b => new VendorBroadcastDto(b.Id.Value, vendorId.Value, vendor.BusinessName, b.Title, b.Message, b.CreatedDateTimeUtc, b.ExpiresAtUtc))
            .ToList();
    }
}
