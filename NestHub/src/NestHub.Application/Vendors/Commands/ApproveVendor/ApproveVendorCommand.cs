using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.ApproveVendor;

public sealed record ApproveVendorCommand(Guid VendorId) : IRequest<Unit>;

public sealed class ApproveVendorCommandValidator : AbstractValidator<ApproveVendorCommand>
{
    public ApproveVendorCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class ApproveVendorCommandHandler : IRequestHandler<ApproveVendorCommand, Unit>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveVendorCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ApproveVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        vendor.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
