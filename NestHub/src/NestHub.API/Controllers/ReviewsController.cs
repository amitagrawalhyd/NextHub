using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NestHub.Application.Reviews.Commands.FlagReview;
using NestHub.Application.Reviews.Commands.RemoveReview;
using NestHub.Application.Reviews.Commands.SubmitReview;
using NestHub.Application.Reviews.Dtos;
using NestHub.Application.Reviews.Queries.GetFlaggedReviews;
using NestHub.Application.Reviews.Queries.GetSocietyReviewsForVendor;

namespace NestHub.API.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly ISender _sender;

    public ReviewsController(ISender sender) => _sender = sender;

    [HttpGet]
    [Authorize(Roles = "Resident,Vendor,Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForVendorInSociety([FromQuery] Guid vendorId, [FromQuery] Guid societyId, CancellationToken cancellationToken)
    {
        var reviews = await _sender.Send(new GetSocietyReviewsForVendorQuery(vendorId, societyId), cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("flagged")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFlagged(CancellationToken cancellationToken)
    {
        var reviews = await _sender.Send(new GetFlaggedReviewsQuery(), cancellationToken);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize(Roles = "Resident")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Submit(SubmitReviewCommand command, CancellationToken cancellationToken)
    {
        var review = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetForVendorInSociety), new { vendorId = review.VendorId, societyId = review.SocietyId }, review);
    }

    [HttpPost("{id:guid}/flag")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Flag(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new FlagReviewCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new RemoveReviewCommand(id), cancellationToken);
        return NoContent();
    }
}
