using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.SosRequests.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.SosRequests.Queries.GetOpenSosRequestsForVendor;

/// <summary>
/// Powers the vendor "Priority SOS Push Listener": open requests matching the vendor's
/// service category within a society, so approved vendors can claim high-intent leads.
/// </summary>
public sealed record GetOpenSosRequestsForVendorQuery(Guid SocietyId, string Category) : IRequest<IReadOnlyList<SosRequestDto>>;

public sealed class GetOpenSosRequestsForVendorQueryHandler : IRequestHandler<GetOpenSosRequestsForVendorQuery, IReadOnlyList<SosRequestDto>>
{
    private readonly ISosRequestRepository _sosRequestRepository;

    public GetOpenSosRequestsForVendorQueryHandler(ISosRequestRepository sosRequestRepository) => _sosRequestRepository = sosRequestRepository;

    public async Task<IReadOnlyList<SosRequestDto>> Handle(GetOpenSosRequestsForVendorQuery request, CancellationToken cancellationToken)
    {
        var requests = await _sosRequestRepository.GetOpenBySocietyAndCategoryAsync(
            new SocietyId(request.SocietyId),
            request.Category,
            cancellationToken);

        return requests.Select(r => r.ToDto()).ToList();
    }
}
