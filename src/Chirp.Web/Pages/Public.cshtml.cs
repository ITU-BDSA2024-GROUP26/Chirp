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
    [BindProperty]
    [StringLength(160, ErrorMessage = "Maximum length is 160")]
    public required string? Message { get; set; }
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    private FollowModel _followModel; 
    // Idea of lazy initialization here is that the User we refer to probably isn't up to date when this class is created. 
    // miight need some kind of logic to check if we should recreate the class if there are changes to the user, but from our tests so far that isn't relevant
    private FollowModel lazyGetFollowModel() 
    {
        if(_followModel == null) { _followModel = new FollowModel(cheepRepository, userManager, User); }
        return _followModel;
    }

    

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }
    
    public async Task<ActionResult> OnPostShareAsync() {
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

    public async Task<IActionResult> OnPostFollowAsync(string UsrnmToFollow) {
        await lazyGetFollowModel().OnPostFollowAsync(UsrnmToFollow); 
        return RedirectToPage("");
    }
}