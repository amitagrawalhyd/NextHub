using FluentValidation;
using MediatR;
using NestHub.Application.Announcements.Dtos;
using NestHub.Domain.Announcements;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommand(Guid SocietyId, string Title, string Body, DateTime? ExpiresAtUtc) : IRequest<AnnouncementDto>;

public sealed class CreateAnnouncementCommandValidator : AbstractValidator<CreateAnnouncementCommand>
{
    public CreateAnnouncementCommandValidator()
    {
        RuleFor(x => x.SocietyId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty();
        RuleFor(x => x.Body).NotEmpty();
    }
}

public sealed class CreateAnnouncementCommandHandler : IRequestHandler<CreateAnnouncementCommand, AnnouncementDto>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAnnouncementCommandHandler(IAnnouncementRepository announcementRepository, IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AnnouncementDto> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = Announcement.Create(new SocietyId(request.SocietyId), request.Title, request.Body, request.ExpiresAtUtc);

        _announcementRepository.Add(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AnnouncementDto(announcement.Id.Value, announcement.SocietyId.Value, announcement.Title, announcement.Body, announcement.CreatedDateTimeUtc, announcement.ExpiresAtUtc);
    }
}
