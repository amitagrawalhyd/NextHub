using MediatR;
using NestHub.Application.Announcements.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Announcements.Queries.GetActiveAnnouncementsForSociety;

public sealed record GetActiveAnnouncementsForSocietyQuery(Guid SocietyId) : IRequest<IReadOnlyList<AnnouncementDto>>;

public sealed class GetActiveAnnouncementsForSocietyQueryHandler : IRequestHandler<GetActiveAnnouncementsForSocietyQuery, IReadOnlyList<AnnouncementDto>>
{
    private readonly IAnnouncementRepository _announcementRepository;

    public GetActiveAnnouncementsForSocietyQueryHandler(IAnnouncementRepository announcementRepository) => _announcementRepository = announcementRepository;

    public async Task<IReadOnlyList<AnnouncementDto>> Handle(GetActiveAnnouncementsForSocietyQuery request, CancellationToken cancellationToken)
    {
        var announcements = await _announcementRepository.GetActiveForSocietyAsync(new SocietyId(request.SocietyId), cancellationToken);
        return announcements
            .Select(a => new AnnouncementDto(a.Id.Value, a.SocietyId.Value, a.Title, a.Body, a.CreatedDateTimeUtc, a.ExpiresAtUtc))
            .ToList();
    }
}
