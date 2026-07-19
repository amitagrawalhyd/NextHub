using MediatR;
using NestHub.Application.Admin.SocietyAdmins.Dtos;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Admin.SocietyAdmins.Queries.GetSocietyAdmins;

public sealed record GetSocietyAdminsQuery : IRequest<IReadOnlyList<SocietyAdminDto>>;

public sealed class GetSocietyAdminsQueryHandler : IRequestHandler<GetSocietyAdminsQuery, IReadOnlyList<SocietyAdminDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISocietyRepository _societyRepository;

    public GetSocietyAdminsQueryHandler(IUserRepository userRepository, ISocietyRepository societyRepository)
    {
        _userRepository = userRepository;
        _societyRepository = societyRepository;
    }

    public async Task<IReadOnlyList<SocietyAdminDto>> Handle(GetSocietyAdminsQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var societies = (await _societyRepository.GetActiveAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);

        return users
            .Where(u => u.UserType == UserType.Admin && u.SocietyId is not null)
            .OrderBy(u => u.PhoneNumber.Value)
            .Select(u => new SocietyAdminDto(
                u.Id.Value,
                u.PhoneNumber.Value,
                u.Email?.Value,
                u.SocietyId!.Value.Value,
                societies.TryGetValue(u.SocietyId!.Value, out var societyName) ? societyName : "(unknown)",
                u.IsActive,
                u.IsVerified))
            .ToList();
    }
}
