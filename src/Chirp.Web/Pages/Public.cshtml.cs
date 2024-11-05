using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor.Pages;

public class PublicModel(ICheepService service) : PageModel
{
    [BindProperty]
    [Required]
    [StringLength(160, ErrorMessage = "Maximum length is 160")]
    public string Message { get; set; }
    private readonly ICheepService _service = service;
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    public async Task<ActionResult> OnGetAsync([FromQuery] int page = 1)
    {
        Cheeps = await _service.GetCheepsAsync(page);
        return Page();
    }
}
