using FluentValidation;
using MediatR;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.RegisterVendor;

public sealed record RegisterVendorCommand(Guid UserId, string BusinessName, string WhatsAppNumber, string? Bio, double? Latitude = null, double? Longitude = null) : IRequest<VendorDto>;

public sealed class RegisterVendorCommandValidator : AbstractValidator<RegisterVendorCommand>
{
    public RegisterVendorCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BusinessName).NotEmpty();
        RuleFor(x => x.WhatsAppNumber).NotEmpty();
    }
}

public sealed class RegisterVendorCommandHandler : IRequestHandler<RegisterVendorCommand, VendorDto>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterVendorCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorDto> Handle(RegisterVendorCommand request, CancellationToken cancellationToken)
    {
        var whatsAppNumber = PhoneNumber.Create(request.WhatsAppNumber);
        var location = request.Latitude.HasValue && request.Longitude.HasValue
            ? GeoLocation.Create(request.Latitude.Value, request.Longitude.Value)
            : null;
        var vendor = Vendor.Register(new UserId(request.UserId), request.BusinessName, whatsAppNumber, request.Bio, OperatingHours.AlwaysOpen(), location);

        _vendorRepository.Add(vendor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.ToDto();
    }
}
