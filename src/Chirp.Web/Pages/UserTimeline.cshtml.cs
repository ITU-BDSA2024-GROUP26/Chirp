using Chirp.Core.DTOs;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel(ICheepRepository cheepRepository, UserManager<Author> userManager, ICheepService service) : TimelineModel(service, cheepRepository, userManager)
{
    public async Task<ActionResult> OnGetAsync(string author, [FromQuery] int page = 1)
    {
        Cheeps = await service.GetCheepsAsync(page, author);
        return Page();
    }
}