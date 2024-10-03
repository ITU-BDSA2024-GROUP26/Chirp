using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly IChirpRepository _service;
    public IEnumerable<CheepDTO> Cheeps { get; set; }

    public UserTimelineModel(IChirpRepository service)
    {
        _service = service;
    }

    public async Task<ActionResult> OnGetAsync(string author, [FromQuery] int page = 1)
    {
        Cheeps = await _service.ReadCheeps(32, (page -1) * 32, author); 
        return Page();
    }
}
