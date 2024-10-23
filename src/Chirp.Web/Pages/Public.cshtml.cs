using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel(ICheepService service) : PageModel
{
    private readonly ICheepService _service = service;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Cheeps = await _service.GetCheepsAsync(page);
        return Page();
    }
}
