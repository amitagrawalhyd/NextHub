using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.SetVendorFeatured;

public sealed record SetVendorFeaturedCommand(Guid VendorId, bool IsFeatured) : IRequest<Unit>;

public sealed class SetVendorFeaturedCommandValidator : AbstractValidator<SetVendorFeaturedCommand>
{
    public SetVendorFeaturedCommandValidator() => RuleFor(x => x.VendorId).NotEmpty();
}

public sealed class SetVendorFeaturedCommandHandler : IRequestHandler<SetVendorFeaturedCommand, Unit>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetVendorFeaturedCommandHandler(IVendorRepository vendorRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SetVendorFeaturedCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(new VendorId(request.VendorId), cancellationToken)
            ?? throw new NotFoundException(nameof(Vendor), request.VendorId);

        if (request.IsFeatured) vendor.MarkFeatured();
        else vendor.UnmarkFeatured();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
