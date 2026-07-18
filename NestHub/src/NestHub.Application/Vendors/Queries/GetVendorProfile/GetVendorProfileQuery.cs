using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Queries.GetVendorProfile;

public sealed record GetVendorProfileQuery(Guid VendorId) : IRequest<VendorDto>;

public sealed class GetVendorProfileQueryHandler : IRequestHandler<GetVendorProfileQuery, VendorDto>
{
    private readonly IVendorRepository _vendorRepository;

    public GetVendorProfileQueryHandler(IVendorRepository vendorRepository) => _vendorRepository = vendorRepository;

    public async Task<VendorDto> Handle(GetVendorProfileQuery request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdWithServicesAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        return vendor.ToDto();
    }
}
