using Chirp.Core.DTOs;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Chirp.Infrastructure.Repositories.Migrations;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication;
using System.Text;


namespace Chirp.Razor.Pages;

[Authorize]
public class AboutMeModel : PageModel
{
    //public string? AuthorName => HttpContext.GetRouteValue("Author")?.ToString();
    private readonly ICheepService _cheepService;
    private readonly ICheepRepository _cheepRepository;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public AboutMeModel(ICheepService cheepService, ICheepRepository cheepRepository, UserManager<Author> userManager, SignInManager<Author> signInManager)
    {
        _cheepService = cheepService;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public AuthorDto? CurrentUser { get; set; }
    public IEnumerable<AuthorDto> FollowingList { get; set; } = new List<AuthorDto>();
    public IEnumerable<CheepDTO> UserCheeps { get; set; } = new List<CheepDTO>();

    // Handler for GET request to populate the page with user data

    public async Task<IActionResult> OnGetAsync()
    {
        // Retrieve the current authenticated user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            // If user is not found, sign out and redirect to public timeline
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Public");
        }

        // Map the user to DTO
        CurrentUser = new AuthorDto(user);

        // Fetch the list of users the current user is following 
        FollowingList = await _cheepService.GetFollowingAuthorsAsync(user.UserName ?? "Unknown");


        // Fetch the user's cheeps
        UserCheeps = await _cheepService.GetCheepsAsync(page: 1, authorRegex: user.UserName);

        return Page();
    }

    public async Task<IActionResult> OnPostDownloadInfoAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Public");
        }

        FollowingList = await _cheepService.GetFollowingAuthorsAsync(user.UserName ?? "Uknown");

        UserCheeps = await _cheepService.GetCheepsAsync(page: 1, authorRegex: user.UserName);


        // Create the textfile
        var content = new StringBuilder();
        content.AppendLine($"{user.UserName}'s information:");
        content.AppendLine($"-----------------------");

        content.AppendLine($"Name: {user.UserName}");

        if (!string.IsNullOrEmpty(user.Email) && user.Email != " ") content.AppendLine($"Email: {user.Email}");
        else content.AppendLine($"Email: No Email");

        content.AppendLine("Following:");
        if (user.FollowingList != null)
        {
            if (user.FollowingList.Any())
            {
                foreach (var follow in user.FollowingList)
                {
                    content.AppendLine($"- {follow.UserName}");
                }
            }
            else content.AppendLine("- No following");
        }
        else content.AppendLine("- No following");

        content.AppendLine("Cheeps:");

        if (UserCheeps.Any())
        {
            foreach (var cheep in UserCheeps)
            {
                content.AppendLine($"- \"{cheep.MessageContent}\" ({cheep.TimeStampStr})");
            }
        }
        else content.AppendLine("- No Cheeps posted yet");

        // Convert content into bytes and return file
        byte[] fileBytes = Encoding.UTF8.GetBytes(content.ToString());
        string fileName = $"{user.UserName}_Chirp_data.txt";
        return File(fileBytes, "text/plain", fileName);
    }

    public async Task<IActionResult> OnPostForgetMeAsync()
    {
        var Author = await _userManager.GetUserAsync(User);

        await _cheepRepository.DeleteAuthorByName(Author.UserName);
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Public");
    }
}

