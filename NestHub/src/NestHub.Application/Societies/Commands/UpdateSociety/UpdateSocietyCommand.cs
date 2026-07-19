using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Societies.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Societies.Commands.UpdateSociety;

public sealed record UpdateSocietyCommand(Guid SocietyId, string Name, string Address, double? Latitude, double? Longitude) : IRequest<SocietyDto>;

public sealed class UpdateSocietyCommandValidator : AbstractValidator<UpdateSocietyCommand>
{
    public UpdateSocietyCommandValidator()
    {
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
    }
}

public sealed class UpdateSocietyCommandHandler : IRequestHandler<UpdateSocietyCommand, SocietyDto>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSocietyCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SocietyDto> Handle(UpdateSocietyCommand request, CancellationToken cancellationToken)
    {
        var society = await _societyRepository.GetByIdAsync(new SocietyId(request.SocietyId), cancellationToken)
            ?? throw new NotFoundException(nameof(Society), request.SocietyId);

        var geoLocation = request.Latitude.HasValue && request.Longitude.HasValue
            ? GeoLocation.Create(request.Latitude.Value, request.Longitude.Value)
            : null;

        society.UpdateDetails(request.Name, request.Address, geoLocation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return society.ToDto();
    }
}
