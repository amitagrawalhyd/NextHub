using MediatR;
using NestHub.Application.Audit.Dtos;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Audit.Queries.GetSystemAudit;

/// <summary>
/// Composite read-only activity feed for the Admin "system audit grid" — assembled from
/// existing tables (no separate audit-log table exists in the schema) so admins can review
/// recent registrations, approvals, moderation actions, and SOS activity in one place.
/// </summary>
public sealed record GetSystemAuditQuery(int MaxEntries = 100) : IRequest<IReadOnlyList<AuditEntryDto>>;

public sealed class GetSystemAuditQueryHandler : IRequestHandler<GetSystemAuditQuery, IReadOnlyList<AuditEntryDto>>
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly ISosRequestRepository _sosRequestRepository;

    public GetSystemAuditQueryHandler(IVendorRepository vendorRepository, IReviewRepository reviewRepository, ISosRequestRepository sosRequestRepository)
    {
        _vendorRepository = vendorRepository;
        _reviewRepository = reviewRepository;
        _sosRequestRepository = sosRequestRepository;
    }

    public async Task<IReadOnlyList<AuditEntryDto>> Handle(GetSystemAuditQuery request, CancellationToken cancellationToken)
    {
        var entries = new List<AuditEntryDto>();

        var pendingVendors = await _vendorRepository.GetPendingApprovalAsync(cancellationToken);
        entries.AddRange(pendingVendors.Select(v =>
            new AuditEntryDto(DateTime.UtcNow, "VendorPendingApproval", $"Vendor '{v.BusinessName}' is awaiting approval.")));

        var flaggedReviews = await _reviewRepository.GetFlaggedAsync(cancellationToken);
        entries.AddRange(flaggedReviews.Select(r =>
            new AuditEntryDto(r.CreatedDateTimeUtc, "ReviewFlagged", $"Review {r.Id.Value} for vendor {r.VendorId.Value} was flagged for moderation.")));

        return entries
            .OrderByDescending(e => e.TimestampUtc)
            .Take(request.MaxEntries)
            .ToList();
    }
}
