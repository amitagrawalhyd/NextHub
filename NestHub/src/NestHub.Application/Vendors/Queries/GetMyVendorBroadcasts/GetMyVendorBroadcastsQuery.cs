using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Queries.GetMyVendorBroadcasts;

public sealed record GetMyVendorBroadcastsQuery(Guid VendorId) : IRequest<IReadOnlyList<VendorBroadcastDto>>;

public sealed class GetMyVendorBroadcastsQueryHandler : IRequestHandler<GetMyVendorBroadcastsQuery, IReadOnlyList<VendorBroadcastDto>>
{
    private readonly IVendorBroadcastRepository _broadcastRepository;
    private readonly IVendorRepository _vendorRepository;

    public GetMyVendorBroadcastsQueryHandler(IVendorBroadcastRepository broadcastRepository, IVendorRepository vendorRepository)
    {
        _broadcastRepository = broadcastRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyList<VendorBroadcastDto>> Handle(GetMyVendorBroadcastsQuery request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var broadcasts = await _broadcastRepository.GetByVendorIdAsync(vendorId, cancellationToken);

        return broadcasts
            .Select(b => new VendorBroadcastDto(b.Id.Value, vendorId.Value, vendor.BusinessName, b.Title, b.Message, b.CreatedDateTimeUtc, b.ExpiresAtUtc))
            .ToList();
    }
}
