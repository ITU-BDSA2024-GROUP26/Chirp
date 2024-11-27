using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

public class PublicModel(ICheepService service, UserManager<Author> userManager) : TimelineModel(service, userManager)
{
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }
}