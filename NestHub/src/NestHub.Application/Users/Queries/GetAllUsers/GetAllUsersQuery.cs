using MediatR;
using NestHub.Application.Users.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<IReadOnlyList<AdminUserDto>>;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IReadOnlyList<AdminUserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<IReadOnlyList<AdminUserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users
            .OrderBy(u => u.UserType)
            .ThenBy(u => u.PhoneNumber.Value)
            .Select(u => new AdminUserDto(u.Id.Value, u.PhoneNumber.Value, u.UserType.ToString(), u.IsVerified, u.IsActive))
            .ToList();
    }
}
