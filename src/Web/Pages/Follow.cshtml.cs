using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Web.Pages;

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