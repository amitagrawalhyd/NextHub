using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetVendorInHouseSociety;

public sealed record GetVendorInHouseSocietyQuery(Guid VendorId) : IRequest<Guid?>;

public sealed class GetVendorInHouseSocietyQueryHandler : IRequestHandler<GetVendorInHouseSocietyQuery, Guid?>
{
    private readonly IVendorSocietyCoverageRepository _coverageRepository;

    public GetVendorInHouseSocietyQueryHandler(IVendorSocietyCoverageRepository coverageRepository) => _coverageRepository = coverageRepository;

    public async Task<Guid?> Handle(GetVendorInHouseSocietyQuery request, CancellationToken cancellationToken)
    {
        var coverage = await _coverageRepository.GetInHouseForVendorAsync(new VendorId(request.VendorId), cancellationToken);
        return coverage?.SocietyId.Value;
    }
}
