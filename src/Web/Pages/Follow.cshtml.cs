using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

/// <summary>
/// The Following page model, which is the page model for the author partial. 
/// Supports the follow endpoint, which allows an authenticated user to follow or unfollow other authors
/// </summary>
/// <param name="service">Our service, injected.</param>
/// <param name="userManager">The usermanager, needed to fetch the current users username.</param>
/// <param name="User">The current user, needed to check whether he is authenticated.</param>
public class FollowModel(ICheepService service, UserManager<Author> userManager, System.Security.Claims.ClaimsPrincipal User)
{
    public async Task OnPostFollowAsync(string UsrnmToFollow) {
        if (UsrnmToFollow == null ||(User.Identity != null && !User.Identity.IsAuthenticated))
        {
            return;
        }

        Author curUser = await userManager!.GetUserAsync(User!);

        await service.AddOrRemoveFollower(curUser.UserName ?? throw new Exception("Wtf user with null username"), UsrnmToFollow); 
        return;
    }
}