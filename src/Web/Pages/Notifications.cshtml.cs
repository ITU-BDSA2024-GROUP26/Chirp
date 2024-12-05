using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace Web.Pages;

public class NotificationsModel (ICheepService service, UserManager<Author> userManager) : PageModel
{
    public IEnumerable<NotificationDTO> notifications; 
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1) 
    {
        if(User.Identity == null || !User.Identity.IsAuthenticated) {
            return RedirectToPage("/Public");
        }
        var author = await userManager.GetUserAsync(User); 
        notifications = await service.GetAuthorsNotifications(author!.UserName!, true); 
        Console.WriteLine($"Length of notifications in pagemodel {notifications.Count()}");
        return Page();
    }

    public async Task<JsonResult> OnGetNewNotifications()
    {
        var author = await userManager.GetUserAsync(User); 
        // Fetch new notifications (replace with your logic)
        var newNotifications = await service.GetAuthorsNotifications(author!.UserName!, false); 
        return new JsonResult(newNotifications);
    }
}