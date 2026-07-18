using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Analytics.Commands.RecordAnalyticsEvent;
using NestHub.Application.Analytics.Dtos;
using NestHub.Application.Analytics.Queries.GetTopVendorsByEngagement;
using NestHub.Application.Analytics.Queries.GetVendorAnalyticsDashboard;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/analytics")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly ISender _sender;

    public AnalyticsController(ISender sender) => _sender = sender;

    [HttpPost("events")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RecordEvent(RecordAnalyticsEventCommand command, CancellationToken cancellationToken)
    {
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("vendors/{vendorId:guid}/dashboard")]
    [Authorize(Roles = "Vendor,Admin")]
    [ProducesResponseType(typeof(AnalyticsSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(Guid vendorId, [FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken cancellationToken)
    {
        var summary = await _sender.Send(new GetVendorAnalyticsDashboardQuery(vendorId, fromUtc, toUtc), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("vendors/top")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<TopVendorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopVendors([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, [FromQuery] int take = 5, CancellationToken cancellationToken = default)
    {
        var topVendors = await _sender.Send(new GetTopVendorsByEngagementQuery(fromUtc, toUtc, take), cancellationToken);
        return Ok(topVendors);
    }
}
