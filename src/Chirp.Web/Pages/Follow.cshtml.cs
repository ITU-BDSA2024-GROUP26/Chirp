using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class FollowModel(ICheepRepository cheepRepository, UserManager<Author> userManager, System.Security.Claims.ClaimsPrincipal User)
{

    public async Task OnPostFollowAsync(string UsrnmToFollow) {

        if (UsrnmToFollow == null ||User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return;
        }

        Author curUser = await userManager.GetUserAsync(User);

        await cheepRepository.AddOrRemoveFollower(curUser.UserName ?? throw new Exception("Wtf user with null username"), UsrnmToFollow); 
        return;

    }
}