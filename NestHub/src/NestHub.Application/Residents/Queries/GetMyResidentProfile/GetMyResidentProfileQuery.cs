using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Residents.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Residents.Queries.GetMyResidentProfile;

public sealed record GetMyResidentProfileQuery(Guid UserId) : IRequest<ResidentDto?>;

public sealed class GetMyResidentProfileQueryHandler : IRequestHandler<GetMyResidentProfileQuery, ResidentDto?>
{
    private readonly IResidentRepository _residentRepository;

    public GetMyResidentProfileQueryHandler(IResidentRepository residentRepository) => _residentRepository = residentRepository;

    public async Task<ResidentDto?> Handle(GetMyResidentProfileQuery request, CancellationToken cancellationToken)
    {
        var resident = await _residentRepository.GetByUserIdAsync(new UserId(request.UserId), cancellationToken);
        return resident?.ToDto();
    }
}
