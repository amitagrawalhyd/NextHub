using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetPendingVendorApprovals;

public sealed record GetPendingVendorApprovalsQuery : IRequest<IReadOnlyList<VendorDto>>;

public sealed class GetPendingVendorApprovalsQueryHandler : IRequestHandler<GetPendingVendorApprovalsQuery, IReadOnlyList<VendorDto>>
{
    private readonly IVendorRepository _vendorRepository;

    public GetPendingVendorApprovalsQueryHandler(IVendorRepository vendorRepository) => _vendorRepository = vendorRepository;

    public async Task<IReadOnlyList<VendorDto>> Handle(GetPendingVendorApprovalsQuery request, CancellationToken cancellationToken)
    {
        var vendors = await _vendorRepository.GetPendingApprovalAsync(cancellationToken);
        return vendors.Select(v => v.ToDto()).ToList();
    }
}
