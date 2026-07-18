using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Societies.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Societies.Queries.GetActiveSocieties;

public sealed record GetActiveSocietiesQuery : IRequest<IReadOnlyList<SocietyDto>>;

public sealed class GetActiveSocietiesQueryHandler : IRequestHandler<GetActiveSocietiesQuery, IReadOnlyList<SocietyDto>>
{
    private readonly ISocietyRepository _societyRepository;

    public GetActiveSocietiesQueryHandler(ISocietyRepository societyRepository) => _societyRepository = societyRepository;

    public async Task<IReadOnlyList<SocietyDto>> Handle(GetActiveSocietiesQuery request, CancellationToken cancellationToken)
    {
        var societies = await _societyRepository.GetActiveAsync(cancellationToken);
        return societies.Select(s => s.ToDto()).ToList();
    }
}
