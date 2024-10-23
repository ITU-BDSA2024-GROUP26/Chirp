using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel(ICheepService service) : PageModel
{
    private readonly ICheepService _service = service;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    public async Task<ActionResult> OnGetAsync(string author, [FromQuery] int page = 1)
    {
        Cheeps = await _service.GetCheepsAsync(page, author);
        return Page();
    }
}