using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Application.Common.Interfaces;
using NestHub.Application.Vendors.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.CreateVendorBroadcast;

public sealed record CreateVendorBroadcastCommand(Guid VendorId, string Title, string Message, DateTime? ExpiresAtUtc) : IRequest<VendorBroadcastDto>;

public sealed class CreateVendorBroadcastCommandValidator : AbstractValidator<CreateVendorBroadcastCommand>
{
    public CreateVendorBroadcastCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Message).NotEmpty();
    }
}

public sealed class CreateVendorBroadcastCommandHandler : IRequestHandler<CreateVendorBroadcastCommand, VendorBroadcastDto>
{
    private readonly IVendorBroadcastRepository _broadcastRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IVendorSocietyCoverageRepository _coverageRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVendorBroadcastCommandHandler(
        IVendorBroadcastRepository broadcastRepository,
        IVendorRepository vendorRepository,
        IVendorSocietyCoverageRepository coverageRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork)
    {
        _broadcastRepository = broadcastRepository;
        _vendorRepository = vendorRepository;
        _coverageRepository = coverageRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorBroadcastDto> Handle(CreateVendorBroadcastCommand request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        var broadcast = VendorBroadcast.Create(vendorId, request.Title, request.Message, request.ExpiresAtUtc);
        _broadcastRepository.Add(broadcast);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var coverage = await _coverageRepository.GetByVendorIdAsync(vendorId, cancellationToken);
        var societyIds = coverage.Select(c => c.SocietyId);

        await _notificationService.BroadcastVendorUpdateAsync(
            broadcast.Id.Value, vendorId, vendor.BusinessName, broadcast.Title, broadcast.Message, societyIds, cancellationToken);

        return new VendorBroadcastDto(broadcast.Id.Value, vendorId.Value, vendor.BusinessName, broadcast.Title, broadcast.Message, broadcast.CreatedDateTimeUtc, broadcast.ExpiresAtUtc);
    }
}
