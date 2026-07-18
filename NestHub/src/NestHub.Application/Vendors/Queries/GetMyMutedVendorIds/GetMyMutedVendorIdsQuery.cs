using MediatR;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Vendors.Queries.GetMyMutedVendorIds;

public sealed record GetMyMutedVendorIdsQuery(Guid ResidentId) : IRequest<IReadOnlyList<Guid>>;

public sealed class GetMyMutedVendorIdsQueryHandler : IRequestHandler<GetMyMutedVendorIdsQuery, IReadOnlyList<Guid>>
{
    private readonly IVendorMuteRepository _muteRepository;

    public GetMyMutedVendorIdsQueryHandler(IVendorMuteRepository muteRepository) => _muteRepository = muteRepository;

    public async Task<IReadOnlyList<Guid>> Handle(GetMyMutedVendorIdsQuery request, CancellationToken cancellationToken)
    {
        var mutes = await _muteRepository.GetByResidentIdAsync(new ResidentId(request.ResidentId), cancellationToken);
        return mutes.Select(m => m.VendorId.Value).ToList();
    }
}
