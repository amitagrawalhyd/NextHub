using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetMyFavoriteVendors;

public sealed record GetMyFavoriteVendorsQuery(Guid ResidentId) : IRequest<IReadOnlyList<VendorDto>>;

public sealed class GetMyFavoriteVendorsQueryHandler : IRequestHandler<GetMyFavoriteVendorsQuery, IReadOnlyList<VendorDto>>
{
    private readonly IVendorFavoriteRepository _favoriteRepository;
    private readonly IVendorRepository _vendorRepository;

    public GetMyFavoriteVendorsQueryHandler(IVendorFavoriteRepository favoriteRepository, IVendorRepository vendorRepository)
    {
        _favoriteRepository = favoriteRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyList<VendorDto>> Handle(GetMyFavoriteVendorsQuery request, CancellationToken cancellationToken)
    {
        var favorites = await _favoriteRepository.GetByResidentIdAsync(new ResidentId(request.ResidentId), cancellationToken);

        var vendors = new List<VendorDto>();
        foreach (var favorite in favorites)
        {
            var vendor = await _vendorRepository.GetByIdWithServicesAsync(favorite.VendorId, cancellationToken);
            if (vendor is not null)
                vendors.Add(vendor.ToDto());
        }

        return vendors;
    }
}
