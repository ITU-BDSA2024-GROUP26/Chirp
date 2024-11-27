using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace Web.Pages;

public class PrivateTimelineModel (ICheepService service, ICheepRepository cheepRepository, UserManager<Author> userManager) : TimelineModel(service, cheepRepository, userManager)
{
    [BindProperty(SupportsGet = true)]
    public string? userName { get; set; }
    public string curPageUserName = ""; 
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        if(User.Identity != null && User.Identity.IsAuthenticated && Author.UserName == userName) {
            Cheeps = Author != null ? await service.GetFollowingCheepsAsync(page, Author?.UserName) : [];
        } else {
            Cheeps = await service.GetCheepsAsync(page, userName ?? "");
        }
        return Page();
    }
}