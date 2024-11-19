using Microsoft.AspNetCore.Mvc.RazorPages;
using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Razor.Pages;

public class FollowModel(ICheepRepository cheepRepository, UserManager<Author> userManager) : PageModel
{

    [BindProperty]
    public required string? UsrnmToFollow { get; set; }

    public async Task<ActionResult> OnPostFollowAsync() {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "ops");
            return Page(); 
        }

        if (UsrnmToFollow == null ||User.Identity != null && !User.Identity.IsAuthenticated)
        {
            return RedirectToPage("");
        }

        Author curUser = await userManager.GetUserAsync(User);

        await cheepRepository.AddOrRemoveFollower(curUser.UserName ?? throw new Exception("Wtf user with null username"), UsrnmToFollow); 
        return RedirectToPage("");

    }
}