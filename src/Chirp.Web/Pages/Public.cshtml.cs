using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class PublicModel(ICheepService service, ICheepRepository cheepRepository, UserManager<Author> userManager) 
    : CheepBoxModel(cheepRepository, userManager)
{
    public Author? Author { get; set; }
    public ICheepRepository CheepRepository = cheepRepository;

    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }
}
