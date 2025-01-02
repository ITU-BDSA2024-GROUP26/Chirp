using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

/// <summary>
/// The page model for the cheep-box partial, which allows the user to send cheeps(via the Share endpoint)
/// </summary>
/// <param name="service">Our Service, injected via DI</param>
/// <param name="userManager">The Usermanager. Also injected by Asp.Net</param>
/// <param name="User">The Current user, needed to check whether the user is authenticated. Also injected by Asp.Net</param>
public class CheepBoxModel(ICheepService service, UserManager<Author> userManager, System.Security.Claims.ClaimsPrincipal User)
{
    public Author? Author { get; set; }
    
    public async Task OnPostShareAsync(string Message) {
        if (User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return ;
        }
        
        // simply truncate the message if too long
        if (Message.Length > 160) { Message = Message.Substring(0, 160); }

        // Create the new Cheep 

        // Save the new Cheep (assuming a SaveCheepAsync method exists in your service or repository)
        await service.SendCheep((await userManager.GetUserAsync(User)).UserName, Message, DateTime.UtcNow);
        return;
    }
}