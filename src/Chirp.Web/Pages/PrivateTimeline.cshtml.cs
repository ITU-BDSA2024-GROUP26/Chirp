using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace Chirp.Razor.Pages;

public class PrivateTimelineModel (ICheepService service, ICheepRepository cheepRepository, UserManager<Author> userManager) 
    : PageModel
{
    public Author? Author { get; set; }
    public ICheepRepository CheepRepository = cheepRepository;

    private CheepBoxModel _cheepBoxModel;
    private CheepBoxModel lazyGetCheepBoxModel() 
    {
        if(_cheepBoxModel == null) 
        {
            _cheepBoxModel = new CheepBoxModel(cheepRepository, userManager, User); 
        }
        return _cheepBoxModel;
    }
    
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
        Console.WriteLine("Yo retard");
        Author = await userManager.GetUserAsync(User);
        Cheeps = Author != null ? await service.GetFollowingCheepsAsync(page, Author?.UserName) : [];
        return Page();
    }

    public async Task<ActionResult> OnPostShareAsync(string Message) 
    {
        await lazyGetCheepBoxModel().OnPostShareAsync(Message); 
        return RedirectToPage("");
    }

    public async Task<IActionResult> OnPostFollowAsync(string UsrnmToFollow) {
        await lazyGetFollowModel().OnPostFollowAsync(UsrnmToFollow); 
        return RedirectToPage("");
    }
}