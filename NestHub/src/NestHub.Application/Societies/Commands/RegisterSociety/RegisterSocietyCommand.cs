using FluentValidation;
using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Societies.Dtos;
using NestHub.Domain.Repositories;
using NestHub.Domain.Societies;
using NestHub.Domain.ValueObjects;

namespace NestHub.Application.Societies.Commands.RegisterSociety;

public sealed record RegisterSocietyCommand(string Name, string Address, double? Latitude, double? Longitude, string City = "Hyderabad") : IRequest<SocietyDto>;

public sealed class RegisterSocietyCommandValidator : AbstractValidator<RegisterSocietyCommand>
{
    public RegisterSocietyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Address).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
    }
}

public sealed class RegisterSocietyCommandHandler : IRequestHandler<RegisterSocietyCommand, SocietyDto>
{
    private readonly ISocietyRepository _societyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterSocietyCommandHandler(ISocietyRepository societyRepository, IUnitOfWork unitOfWork)
    {
        _societyRepository = societyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SocietyDto> Handle(RegisterSocietyCommand request, CancellationToken cancellationToken)
    {
        var geoLocation = request.Latitude.HasValue && request.Longitude.HasValue
            ? GeoLocation.Create(request.Latitude.Value, request.Longitude.Value)
            : null;

        var society = Society.Register(request.Name, request.Address, geoLocation, request.City);
        _societyRepository.Add(society);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return society.ToDto();
    }
}
