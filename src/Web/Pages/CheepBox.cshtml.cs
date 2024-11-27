using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

public class CheepBoxModel(ICheepRepository cheepRepository, UserManager<Author> userManager, System.Security.Claims.ClaimsPrincipal User)
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
        var newCheep = new Cheep
        {
            Author = await userManager.GetUserAsync(User),                    // Set the Author
            Text = Message,                     // Set the text from the form input (Message property)
            TimeStamp = DateTime.UtcNow         // Set the current timestamp
        };

        // Save the new Cheep (assuming a SaveCheepAsync method exists in your service or repository)
        await cheepRepository.CreateCheep(newCheep);
        return;
    }
}