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

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Author = await userManager.GetUserAsync(User);
        Cheeps = await service.GetCheepsAsync(page);
        return Page();
    }

    public async Task<ActionResult> OnPostShareAsync(string Message) 
    {
        await lazyGetCheepBoxModel().OnPostShareAsync(Message); 
        return RedirectToPage("");
    }
}
