using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NestHub.Application.Reviews.Commands.RemoveReview;
using NestHub.Application.Reviews.Dtos;
using NestHub.Application.Reviews.Queries.GetFlaggedReviews;

namespace NestHub.Admin.Pages.Reviews;

public sealed class IndexModel : PageModel
{
    private readonly ISender _sender;

    public IndexModel(ISender sender) => _sender = sender;

    public IReadOnlyList<ReviewDto> FlaggedReviews { get; private set; } = Array.Empty<ReviewDto>();

    public async Task OnGetAsync() => FlaggedReviews = await _sender.Send(new GetFlaggedReviewsQuery());

    public async Task<IActionResult> OnPostRemoveAsync(Guid reviewId)
    {
        await _sender.Send(new RemoveReviewCommand(reviewId));
        return RedirectToPage();
    }
}
