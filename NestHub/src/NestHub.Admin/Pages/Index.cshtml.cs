using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NestHub.Admin.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Vendors/Index");
}
