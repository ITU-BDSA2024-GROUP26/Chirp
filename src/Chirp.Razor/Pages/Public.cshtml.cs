using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IChirpRepository _repo;
    public IEnumerable<CheepDTO> Cheeps { get; set; }

    public PublicModel(IChirpRepository repo)
    {
        _repo = repo;
    }

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Cheeps = await _repo.ReadCheeps(32, (page -1) * 32, null); 
        return Page(); 
    }
}
