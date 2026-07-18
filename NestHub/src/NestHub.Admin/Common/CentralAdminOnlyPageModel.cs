using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestHub.Admin.Common;

/// <summary>
/// Base for pages that manage platform-wide concerns (vendor approvals, categories, user
/// accounts, system audit) rather than a single society. A society-scoped Admin is bounced back
/// to the Dashboard before any handler runs — this is enforced server-side, not just hidden nav.
/// </summary>
public abstract class CentralAdminOnlyPageModel : PageModel
{
    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!User.IsCentralAdmin())
        {
            context.Result = RedirectToPage("/Index");
            return;
        }

        base.OnPageHandlerExecuting(context);
    }
}
