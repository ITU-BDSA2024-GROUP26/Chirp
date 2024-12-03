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
        notifications = await service.GetAuthorsNotifications(author!.UserName!); 
        Console.WriteLine($"Length of notifications in pagemodel {notifications.Count()}");
        return Page();
    }
    
}