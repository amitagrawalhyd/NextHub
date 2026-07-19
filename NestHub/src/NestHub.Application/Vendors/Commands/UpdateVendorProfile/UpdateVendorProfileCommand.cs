using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.UpdateVendorProfile;

public sealed record UpdateVendorProfileCommand(Guid VendorId, string BusinessName, string? Bio, string? LogoUrl, string WhatsAppNumber) : IRequest<VendorDto>;

public sealed class UpdateVendorProfileCommandValidator : AbstractValidator<UpdateVendorProfileCommand>
{
    public UpdateVendorProfileCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.BusinessName).NotEmpty();
        RuleFor(x => x.WhatsAppNumber).NotEmpty();
    }
}

public sealed class UpdateVendorProfileCommandHandler : IRequestHandler<UpdateVendorProfileCommand, VendorDto>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorProfileCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorDto> Handle(UpdateVendorProfileCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdWithServicesAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var whatsAppNumber = PhoneNumber.Create(request.WhatsAppNumber);

        vendor.UpdateProfile(request.BusinessName, request.Bio, request.LogoUrl, whatsAppNumber, vendor.OperatingHours);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.ToDto();
    }
}
