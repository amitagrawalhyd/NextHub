using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetVendorCoverage;

public sealed record GetVendorCoverageQuery(Guid VendorId) : IRequest<IReadOnlyList<Guid>>;

public sealed class GetVendorCoverageQueryHandler : IRequestHandler<GetVendorCoverageQuery, IReadOnlyList<Guid>>
{
    private readonly IVendorSocietyCoverageRepository _coverageRepository;

    public GetVendorCoverageQueryHandler(IVendorSocietyCoverageRepository coverageRepository) => _coverageRepository = coverageRepository;

    public async Task<IReadOnlyList<Guid>> Handle(GetVendorCoverageQuery request, CancellationToken cancellationToken)
    {
        var coverage = await _coverageRepository.GetByVendorIdAsync(new VendorId(request.VendorId), cancellationToken);
        return coverage.Select(c => c.SocietyId.Value).ToList();
    }
}
