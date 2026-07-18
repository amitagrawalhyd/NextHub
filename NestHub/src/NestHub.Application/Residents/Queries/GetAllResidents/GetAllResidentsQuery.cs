using MediatR;
using NestHub.Application.Residents.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Residents.Queries.GetAllResidents;

public sealed record GetAllResidentsQuery(Guid? SocietyId = null) : IRequest<IReadOnlyList<AdminResidentDto>>;

public sealed class GetAllResidentsQueryHandler : IRequestHandler<GetAllResidentsQuery, IReadOnlyList<AdminResidentDto>>
{
    private readonly IResidentRepository _residentRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IUserRepository _userRepository;

    public GetAllResidentsQueryHandler(
        IResidentRepository residentRepository,
        ISocietyRepository societyRepository,
        IUserRepository userRepository)
    {
        _residentRepository = residentRepository;
        _societyRepository = societyRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AdminResidentDto>> Handle(GetAllResidentsQuery request, CancellationToken cancellationToken)
    {
        var residents = await _residentRepository.GetAllAsync(cancellationToken);
        var societies = (await _societyRepository.GetActiveAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);
        var users = (await _userRepository.GetAllAsync(cancellationToken)).ToDictionary(u => u.Id, u => u);

        if (request.SocietyId is { } societyId)
            residents = residents.Where(r => r.SocietyId.Value == societyId).ToList();

        return residents
            .OrderBy(r => r.Name)
            .Select(r => new AdminResidentDto(
                r.Id.Value,
                r.Name,
                users.TryGetValue(r.UserId, out var user) ? user.PhoneNumber.Value : "(unknown)",
                societies.TryGetValue(r.SocietyId, out var societyName) ? societyName : "(unknown)",
                r.BlockNumber,
                r.FlatNumber,
                users.TryGetValue(r.UserId, out var u2) && u2.IsActive))
            .ToList();
    }
}
