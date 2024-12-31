using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace Web.Pages;

/// <summary>
/// The pagemodel for the private timeline endpoint. 
/// Inherits from TimelineModel, which does most of the heavy lifting. 
/// All we need to do is ensure that if an authenticated user views his own timeline, he gets both his own cheeps, the cheeps of everyone he follows and the cheeps wherein he is tagged. 
/// </summary>
/// <param name="service">Our service, injected.</param>
/// <param name="userManager">The usermanager, needed to get the authors username.</param>
public class PrivateTimelineModel (ICheepService service, UserManager<Author> userManager) : TimelineModel(service, userManager)
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