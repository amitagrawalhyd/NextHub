using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetMyVendorProfile;

public sealed record GetMyVendorProfileQuery(Guid UserId) : IRequest<VendorDto?>;

public sealed class GetMyVendorProfileQueryHandler : IRequestHandler<GetMyVendorProfileQuery, VendorDto?>
{
    private readonly IVendorRepository _vendorRepository;

    public GetMyVendorProfileQueryHandler(IVendorRepository vendorRepository) => _vendorRepository = vendorRepository;

    public async Task<VendorDto?> Handle(GetMyVendorProfileQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);
        return vendor?.ToDto();
    }
}
