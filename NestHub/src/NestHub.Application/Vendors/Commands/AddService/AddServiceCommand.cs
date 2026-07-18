using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Mapping;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;
using NestHub.Domain.ValueObjects;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.AddService;

public sealed record AddServiceCommand(
    Guid VendorId,
    string Title,
    string Description,
    string Category,
    PricingType PricingType,
    decimal? Amount,
    string Currency = "INR") : IRequest<ServiceDto>;

public sealed class AddServiceCommandValidator : AbstractValidator<AddServiceCommand>
{
    public AddServiceCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Category).NotEmpty();
        RuleFor(x => x.PricingType).IsInEnum();
        RuleFor(x => x.Amount)
            .NotNull()
            .When(x => x.PricingType != PricingType.ContactForQuote)
            .WithMessage("Amount is required unless pricing is 'Contact for Quote'.");
    }
}

public sealed class AddServiceCommandHandler : IRequestHandler<AddServiceCommand, ServiceDto>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddServiceCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceDto> Handle(AddServiceCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdWithServicesAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var pricing = request.PricingType switch
        {
            PricingType.Fixed => PricingInfo.Fixed(request.Amount!.Value, request.Currency),
            PricingType.Hourly => PricingInfo.Hourly(request.Amount!.Value, request.Currency),
            PricingType.StartingFrom => PricingInfo.StartingFrom(request.Amount!.Value, request.Currency),
            _ => PricingInfo.ContactForQuote()
        };

        var service = vendor.AddService(request.Title, request.Description, pricing, request.Category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return service.ToDto();
    }
}
