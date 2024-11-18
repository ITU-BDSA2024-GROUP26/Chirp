using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class PublicModel(ICheepService service, ICheepRepository cheepRepository, UserManager<Author> userManager) : PageModel
{
    public Author? Author { get; set; }
    public string? Message { get; set; }
    [BindProperty]
    [StringLength(160, ErrorMessage = "Maximum length is 160")]
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }
    
    public async Task<ActionResult> OnPostAsync() {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "ops");
            return Page(); 
        }

        if (User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return RedirectToPage("");
        }
        
        // Create the new Cheep
        var newCheep = new Cheep
        {
            Author = await userManager.GetUserAsync(User),                    // Set the Author
            Text = Message,                     // Set the text from the form input (Message property)
            TimeStamp = DateTime.UtcNow         // Set the current timestamp
        };

        // Save the new Cheep (assuming a SaveCheepAsync method exists in your service or repository)
        await cheepRepository.CreateCheep(newCheep);
        return RedirectToPage("");
    }
}