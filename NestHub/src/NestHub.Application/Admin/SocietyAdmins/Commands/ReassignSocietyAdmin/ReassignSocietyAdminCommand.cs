using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;
using NestHub.Domain.Users;

namespace NestHub.Application.Admin.SocietyAdmins.Commands.ReassignSocietyAdmin;

public sealed record ReassignSocietyAdminCommand(Guid UserId, Guid SocietyId) : IRequest<Unit>;

public sealed class ReassignSocietyAdminCommandValidator : AbstractValidator<ReassignSocietyAdminCommand>
{
    public ReassignSocietyAdminCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SocietyId).NotEmpty();
    }
}

public sealed class ReassignSocietyAdminCommandHandler : IRequestHandler<ReassignSocietyAdminCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReassignSocietyAdminCommandHandler(IUserRepository userRepository, ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ReassignSocietyAdminCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        if (user.UserType != UserType.Admin)
            throw new InvalidOperationException("Only Admin users can be assigned to a society.");

        var societyId = new SocietyId(request.SocietyId);
        _ = await _societyRepository.GetByIdAsync(societyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        user.AssignToSociety(societyId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
