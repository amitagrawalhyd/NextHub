using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Users;

namespace NestHub.Application.Users.Commands.SetUserActive;

public sealed record SetUserActiveCommand(Guid UserId, bool IsActive) : IRequest<Unit>;

public sealed class SetUserActiveCommandValidator : AbstractValidator<SetUserActiveCommand>
{
    public SetUserActiveCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}

public sealed class SetUserActiveCommandHandler : IRequestHandler<SetUserActiveCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserActiveCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (request.IsActive) user.Activate();
        else user.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
