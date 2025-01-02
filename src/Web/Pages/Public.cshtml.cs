using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

/// <summary>
/// The model for the public timeline. Inherits from TimeLineModel which does all the heavy lifting of users following eachother, sending cheeps ect. 
/// Just needs to fetch the current author and current cheps, whith an endpoint that allows one to specify which page. 
/// </summary>
/// <param name="service">Our service, injected</param>
/// <param name="userManager">The usermanager, needed for fetching the author for following purposes.</param>
public class PublicModel(ICheepService service, UserManager<Author> userManager) : TimelineModel(service, userManager)
{
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }
}