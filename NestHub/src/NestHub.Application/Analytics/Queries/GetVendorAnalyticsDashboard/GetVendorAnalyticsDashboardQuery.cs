using FluentValidation;
using MediatR;
using NestHub.Application.Analytics.Dtos;
using NestHub.Domain.Common;
using NestHub.Domain.Enums;
using NestHub.Domain.Repositories;

namespace NestHub.Application.Analytics.Queries.GetVendorAnalyticsDashboard;

public sealed record GetVendorAnalyticsDashboardQuery(Guid VendorId, DateTime FromUtc, DateTime ToUtc) : IRequest<AnalyticsSummaryDto>;

public sealed class GetVendorAnalyticsDashboardQueryValidator : AbstractValidator<GetVendorAnalyticsDashboardQuery>
{
    public GetVendorAnalyticsDashboardQueryValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.ToUtc).GreaterThanOrEqualTo(x => x.FromUtc);
    }
}

public sealed class GetVendorAnalyticsDashboardQueryHandler : IRequestHandler<GetVendorAnalyticsDashboardQuery, AnalyticsSummaryDto>
{
    private readonly IAnalyticsLogRepository _analyticsLogRepository;

    public GetVendorAnalyticsDashboardQueryHandler(IAnalyticsLogRepository analyticsLogRepository) => _analyticsLogRepository = analyticsLogRepository;

    public async Task<AnalyticsSummaryDto> Handle(GetVendorAnalyticsDashboardQuery request, CancellationToken cancellationToken)
    {
        var vendorId = new VendorId(request.VendorId);
        var logs = await _analyticsLogRepository.GetByVendorAsync(vendorId, request.FromUtc, request.ToUtc, cancellationToken);

        return new AnalyticsSummaryDto(
            request.VendorId,
            logs.Count(l => l.ActionType == AnalyticsActionType.ProfileView),
            logs.Count(l => l.ActionType == AnalyticsActionType.CallClick),
            logs.Count(l => l.ActionType == AnalyticsActionType.WhatsAppClick),
            request.FromUtc,
            request.ToUtc);
    }
}
