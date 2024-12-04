using Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;



namespace Web.Pages;

[Authorize]
public class SearchModel : PageModel
{
    UserManager<Author> _userManager;

    public SearchModel(UserManager<Author> userManager)
    {
        _userManager = userManager;
    }


    public string SearchPhrase { get; set; }

    public IEnumerable<AuthorDto> SearchResults { get; set; } = new List<AuthorDto>();

    public async Task<IActionResult> SearchAsync(string SearchPhrase)
    {
        //_userManager.Users.FirstOrDefaultAsync(a => a.UserName == name);

        var users = from u in _userManager.Users
                    where u.UserName.Contains(SearchPhrase)
                    select u;

        return Page();
    }

}

