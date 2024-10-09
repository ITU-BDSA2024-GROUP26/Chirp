using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly ICheepService _service;
    public IEnumerable<CheepDTO> Cheeps { get; set; }

    public UserTimelineModel(ICheepService service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGetAsync(string author, [FromQuery] int page = 1)
    {
        Cheeps = await _service.GetCheepsAsync(page, author);
        return Page();
    }
}
