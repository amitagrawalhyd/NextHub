using MediatR;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetMostFavoritedVendors;

public sealed record MostFavoritedVendorDto(Guid VendorId, string BusinessName, int FavoriteCount);

public sealed record GetMostFavoritedVendorsQuery(int Top = 5) : IRequest<IReadOnlyList<MostFavoritedVendorDto>>;

public sealed class GetMostFavoritedVendorsQueryHandler : IRequestHandler<GetMostFavoritedVendorsQuery, IReadOnlyList<MostFavoritedVendorDto>>
{
    private readonly IVendorFavoriteRepository _favoriteRepository;
    private readonly IVendorRepository _vendorRepository;

    public GetMostFavoritedVendorsQueryHandler(IVendorFavoriteRepository favoriteRepository, IVendorRepository vendorRepository)
    {
        _favoriteRepository = favoriteRepository;
        _vendorRepository = vendorRepository;
    }

    public async Task<IReadOnlyList<MostFavoritedVendorDto>> Handle(GetMostFavoritedVendorsQuery request, CancellationToken cancellationToken)
    {
        var favorites = await _favoriteRepository.GetAllAsync(cancellationToken);
        var grouped = favorites.GroupBy(f => f.VendorId).OrderByDescending(g => g.Count()).Take(request.Top);

        var result = new List<MostFavoritedVendorDto>();
        foreach (var group in grouped)
        {
            var vendor = await _vendorRepository.GetByIdAsync(group.Key, cancellationToken);
            if (vendor is not null)
                result.Add(new MostFavoritedVendorDto(vendor.Id.Value, vendor.BusinessName, group.Count()));
        }

        return result;
    }
}
