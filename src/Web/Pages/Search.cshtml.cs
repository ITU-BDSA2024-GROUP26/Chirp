using Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Infrastructure;
using Microsoft.EntityFrameworkCore;



namespace Web.Pages;

[Authorize]
public class SearchModel : PageModel
{
    private readonly UserManager<Author> _userManager;

    public SearchModel(UserManager<Author> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public string SearchPhrase { get; set; }
    public AuthorDto? CurrentUser { get; set; }
    public IEnumerable<AuthorDto> SearchResults { get; set; } = new List<AuthorDto>();


    public async Task<IActionResult> OnPostSearchAsync(string SearchPhrase)
    {
        if (string.IsNullOrWhiteSpace(SearchPhrase))
        {
            ModelState.AddModelError(string.Empty, "Search phrase cannot be empty.");
            return Page();
        }

        var users = await _userManager.Users
            .Where(u => u.UserName.Contains(SearchPhrase))
            .Select(u => new AuthorDto(u.UserName, null, null))
            .ToListAsync();

        SearchResults = users;

        return Page();
    }
    /*//_userManager.Users.FirstOrDefaultAsync(a => a.UserName == name);

    var users = from u in _userManager.Users
                where u.UserName.Contains(SearchPhrase)
                select u;

    users = (IQueryable<Author>)await users.ToListAsync();
    return Page();
*/
}



