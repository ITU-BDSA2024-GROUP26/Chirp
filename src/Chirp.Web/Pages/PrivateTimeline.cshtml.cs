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

public class PrivateTimelineModel (ICheepService service, ICheepRepository cheepRepository, UserManager<Author> userManager) : TimelineModel(service, cheepRepository, userManager)
{
    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = Author != null ? await service.GetFollowingCheepsAsync(page, Author?.UserName) : [];
        return Page();
    }
}