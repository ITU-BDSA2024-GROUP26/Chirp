using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

/// <summary>
/// Base class for all timelines, containing the functionality both timelines use(Author and Cheep fields, following, sending cheeps)
/// This class uses lazy initialization, for the simple reason that an unauthenticated user can view a timeline just fine. 
/// Thus we only generate the models requiring an authenticated user when we need them, i.e. when the user is authenticated. 
/// If a user manually sends post requests to these endpoints he will get a 500 response due to being unauthenticated, which is not an issue as that isn't normal user behaviour. 
/// </summary>
/// <param name="service">Our service, injected</param>
/// <param name="userManager">The usermanager, which is required for the respective sending and following models</param>
public class TimelineModel(ICheepService service, UserManager<Author> userManager) 
    : PageModel
{
    public Author? Author { get; set; }
    public required IEnumerable<CheepDTO> Cheeps { get; set; }

    private CheepBoxModel? _cheepBoxModel;
    private CheepBoxModel lazyGetCheepBoxModel() 
    {
        if(_cheepBoxModel == null) 
        {
            _cheepBoxModel = new CheepBoxModel(service, userManager, User); 
        }
        return _cheepBoxModel;
    }
    

    private FollowModel? _followModel; 
    // Idea of lazy initialization here is that the User we refer to probably isn't up to date when this class is created. 
    // miight need some kind of logic to check if we should recreate the class if there are changes to the user, but from our tests so far that isn't relevant
    private FollowModel lazyGetFollowModel() 
    {
        if(_followModel == null) { _followModel = new FollowModel(service, userManager, User); }
        return _followModel;
    }

    public async Task<ActionResult> OnPostShareAsync(string Message) 
    {
        await lazyGetCheepBoxModel().OnPostShareAsync(Message); 
        return RedirectToPage("");
    }

    public async Task<IActionResult> OnPostFollowAsync(string UsrnmToFollow) {
        await lazyGetFollowModel().OnPostFollowAsync(UsrnmToFollow); 
        return RedirectToPage("");
    }
}