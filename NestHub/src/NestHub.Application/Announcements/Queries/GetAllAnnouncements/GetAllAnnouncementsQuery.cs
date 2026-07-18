using MediatR;
using NestHub.Application.Announcements.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Announcements.Queries.GetAllAnnouncements;

public sealed record GetAllAnnouncementsQuery(Guid? SocietyId = null) : IRequest<IReadOnlyList<AdminAnnouncementDto>>;

public sealed class GetAllAnnouncementsQueryHandler : IRequestHandler<GetAllAnnouncementsQuery, IReadOnlyList<AdminAnnouncementDto>>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly ISocietyRepository _societyRepository;

    public GetAllAnnouncementsQueryHandler(IAnnouncementRepository announcementRepository, ISocietyRepository societyRepository)
    {
        _announcementRepository = announcementRepository;
        _societyRepository = societyRepository;
    }

    public async Task<IReadOnlyList<AdminAnnouncementDto>> Handle(GetAllAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var announcements = await _announcementRepository.GetAllAsync(cancellationToken);
        var societies = (await _societyRepository.GetActiveAsync(cancellationToken)).ToDictionary(s => s.Id, s => s.Name);

        if (request.SocietyId is { } societyId)
            announcements = announcements.Where(a => a.SocietyId.Value == societyId).ToList();

        return announcements
            .Select(a => new AdminAnnouncementDto(
                a.Id.Value,
                societies.TryGetValue(a.SocietyId, out var name) ? name : "(unknown)",
                a.Title,
                a.Body,
                a.CreatedDateTimeUtc,
                a.ExpiresAtUtc,
                a.IsActive))
            .ToList();
    }
}
