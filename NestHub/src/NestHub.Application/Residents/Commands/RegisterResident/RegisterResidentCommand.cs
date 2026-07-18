using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Residents.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Residents;
using NestHub.Domain.Societies;

namespace NestHub.Application.Residents.Commands.RegisterResident;

public sealed record RegisterResidentCommand(Guid UserId, Guid SocietyId, string BlockNumber, string FlatNumber, string Name) : IRequest<ResidentDto>;

public sealed class RegisterResidentCommandValidator : AbstractValidator<RegisterResidentCommand>
{
    public RegisterResidentCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.BlockNumber).NotEmpty();
        RuleFor(x => x.FlatNumber).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
    }
}

public sealed class RegisterResidentCommandHandler : IRequestHandler<RegisterResidentCommand, ResidentDto>
{
    private readonly IResidentRepository _residentRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterResidentCommandHandler(IResidentRepository residentRepository, ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _residentRepository = residentRepository;
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResidentDto> Handle(RegisterResidentCommand request, CancellationToken cancellationToken)
    {
        var societyId = new SocietyId(request.SocietyId);
        _ = await _societyRepository.GetByIdAsync(societyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        var resident = Resident.Create(new UserId(request.UserId), societyId, request.BlockNumber, request.FlatNumber, request.Name);
        _residentRepository.Add(resident);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return resident.ToDto();
    }
}
