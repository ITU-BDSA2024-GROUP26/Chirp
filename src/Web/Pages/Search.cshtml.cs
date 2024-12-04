using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;



namespace Web.Pages;

[Authorize]
public class SearchModel : PageModel
{
    private readonly CheepDbContext _context;

    public SearchModel(CheepDbContext context)
    {
        _context = context;
    }


    public string SearchPhrase { get; set; }

    public IEnumerable<AuthorDto> SearchResults { get; set; } = new List<AuthorDto>();

    public async Task<IActionResult> Index(string SearchPhrase)
    {
        var users = from u in _context.Users
                    select u;
        if (!string.IsNullOrEmpty(SearchPhrase))
        {
            users = users.Where(u => u.UserName.Contains(SearchPhrase));
            return Page();
        }

        return Page();
    }

    /*    public async Task<IActionResult> Create()
        {
            ViewData["Status"] = new SelectList(_context.Users, "userName"); 
            return Page(); 
        }
        public async Task<IActionResult> OnPostSearchAsync()
        {
            if (ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var users = await _userManager.Users
                .Where(u => EF.Functions.Like(u.UserName, $"%{SearchPhrase}%"))
                .ToListAsync();

                SearchResults = users.Select(u => new AuthorDto(u));

                Console.WriteLine("{users}");

                return Page();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occured while searching. Please try again later");
                return Page();
            }

        }*/

}

