using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;

namespace NestHub.Application.Societies.Commands.SetSocietyActive;

public sealed record SetSocietyActiveCommand(Guid SocietyId, bool IsActive) : IRequest<Unit>;

public sealed class SetSocietyActiveCommandValidator : AbstractValidator<SetSocietyActiveCommand>
{
    public SetSocietyActiveCommandValidator() => RuleFor(x => x.SocietyId).NotEmpty();
}

public sealed class SetSocietyActiveCommandHandler : IRequestHandler<SetSocietyActiveCommand, Unit>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetSocietyActiveCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetSocietyActiveCommand request, CancellationToken cancellationToken)
    {
        var society = await _societyRepository.GetByIdAsync(new SocietyId(request.SocietyId), cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        if (request.IsActive) society.Activate();
        else society.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
