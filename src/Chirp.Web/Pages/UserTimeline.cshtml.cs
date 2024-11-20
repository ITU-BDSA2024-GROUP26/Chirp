using Chirp.Core.DTOs;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel(ICheepRepository cheepRepository, UserManager<Author> userManager, ICheepService service) : PageModel
{
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    private FollowModel _followModel; 
    private FollowModel lazyGetFollowModel() 
    {
        if(_followModel == null) { _followModel = new FollowModel(cheepRepository, userManager, User); }
        return _followModel;
    }

    public async Task<ActionResult> OnGetAsync(string author, [FromQuery] int page = 1)
    {
        Cheeps = await service.GetCheepsAsync(page, author);
        return Page();
    }

    public async Task<IActionResult> OnPostFollowAsync(string UsrnmToFollow) {
        await lazyGetFollowModel().OnPostFollowAsync(UsrnmToFollow); 
        return RedirectToPage("");
    }
}