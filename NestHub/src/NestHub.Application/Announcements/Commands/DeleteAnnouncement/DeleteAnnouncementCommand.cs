using FluentValidation;
using MediatR;
using NestHub.Application.Common.Exceptions;
using NestHub.Domain.Announcements;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Announcements.Commands.DeleteAnnouncement;

public sealed record DeleteAnnouncementCommand(Guid AnnouncementId) : IRequest<Unit>;

public sealed class DeleteAnnouncementCommandValidator : AbstractValidator<DeleteAnnouncementCommand>
{
    public DeleteAnnouncementCommandValidator() => RuleFor(x => x.AnnouncementId).NotEmpty();
}

public sealed class DeleteAnnouncementCommandHandler : IRequestHandler<DeleteAnnouncementCommand, Unit>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnnouncementCommandHandler(IAnnouncementRepository announcementRepository, IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _announcementRepository.GetByIdAsync(new Domain.Common.AnnouncementId(request.AnnouncementId), cancellationToken)
            ?? throw new NotFoundException(nameof(Announcement), request.AnnouncementId);

        _announcementRepository.Remove(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
