using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using NuGet.Protocol;

namespace Web.Pages;

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