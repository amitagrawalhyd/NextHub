using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;
using NestHub.Domain.Vendors;

namespace NestHub.Application.Vendors.Commands.DeleteVendorBroadcast;

public sealed record DeleteVendorBroadcastCommand(Guid BroadcastId) : IRequest<Unit>;

public sealed class DeleteVendorBroadcastCommandValidator : AbstractValidator<DeleteVendorBroadcastCommand>
{
    public DeleteVendorBroadcastCommandValidator() => RuleFor(x => x.BroadcastId).NotEmpty();
}

public sealed class DeleteVendorBroadcastCommandHandler : IRequestHandler<DeleteVendorBroadcastCommand, Unit>
{
    private readonly IVendorBroadcastRepository _broadcastRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVendorBroadcastCommandHandler(IVendorBroadcastRepository broadcastRepository, IUnitOfWork unitOfWork)
    {
        _broadcastRepository = broadcastRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteVendorBroadcastCommand request, CancellationToken cancellationToken)
    {
        var broadcast = await _broadcastRepository.GetByIdAsync(new VendorBroadcastId(request.BroadcastId), cancellationToken)
            ?? throw new NotFoundException(nameof(VendorBroadcast), request.BroadcastId);

        _broadcastRepository.Remove(broadcast);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
