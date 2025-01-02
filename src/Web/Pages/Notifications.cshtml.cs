using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NuGet.Protocol;

namespace Web.Pages;

/// <summary>
/// The page model for the nofications page, which supports fetching notifications, both a one-time fetch of all the notification and periodic fetches of only new notifications
/// </summary>
/// <param name="service">Our service, injected</param>
/// <param name="userManager">The usermanager, needed to fetch the username of the current user</param>
public class NotificationsModel (ICheepService service, UserManager<Author> userManager) : PageModel
{
    public IEnumerable<NotificationDTO> Notifications; 
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1) 
    {
        if(User.Identity == null || !User.Identity.IsAuthenticated) {
            return RedirectToPage("/Public");
        }
        var author = await userManager.GetUserAsync(User); 
        Notifications = await service.GetAuthorsNotifications(author!.UserName!, true); 
        return Page();
    }


    // partly chatgpt(to go with the ajax script) 
    public async Task<JsonResult> OnGetNewNotifications()
    {
        var author = await userManager.GetUserAsync(User); 
        var newNotifications = await service.GetAuthorsNotifications(author!.UserName!, false); 
        
        var res = new JsonResult(newNotifications); 

        return new JsonResult(newNotifications);
    }
}