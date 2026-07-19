using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Residents.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Residents;
using NestHub.Domain.Societies;

namespace NestHub.Application.Residents.Commands.UpdateResident;

public sealed record UpdateResidentCommand(Guid ResidentId, string Name, string BlockNumber, string FlatNumber, Guid SocietyId) : IRequest<ResidentDto>;

public sealed class UpdateResidentCommandValidator : AbstractValidator<UpdateResidentCommand>
{
    public UpdateResidentCommandValidator()
    {
        RuleFor(x => x.ResidentId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.BlockNumber).NotEmpty();
        RuleFor(x => x.FlatNumber).NotEmpty();
        RuleFor(x => x.SocietyId).NotEmpty();
    }
}

public sealed class UpdateResidentCommandHandler : IRequestHandler<UpdateResidentCommand, ResidentDto>
{
    private readonly IResidentRepository _residentRepository;
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateResidentCommandHandler(IResidentRepository residentRepository, ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _residentRepository = residentRepository;
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResidentDto> Handle(UpdateResidentCommand request, CancellationToken cancellationToken)
    {
        var resident = await _residentRepository.GetByIdAsync(new ResidentId(request.ResidentId), cancellationToken)
            ?? throw new NotFoundException(nameof(Resident), request.ResidentId);

        var societyId = new SocietyId(request.SocietyId);
        _ = await _societyRepository.GetByIdAsync(societyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        resident.UpdateDetails(request.Name, request.BlockNumber, request.FlatNumber, societyId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return resident.ToDto();
    }
}
