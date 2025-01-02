using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Migrations;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication;
using System.Text;


namespace Web.Pages;

/// <summary>
///  The page model for the about-me page, which is responsible for fetching all the users data, as well as deleting it if he so desires. 
/// </summary>
[Authorize]
public class AboutMeModel : PageModel
{
    //public string? AuthorName => HttpContext.GetRouteValue("Author")?.ToString();
    private readonly ICheepService _cheepService;
    private readonly UserManager<Author> _userManager;
    private readonly SignInManager<Author> _signInManager;

    public AboutMeModel(ICheepService cheepService, UserManager<Author> userManager, SignInManager<Author> signInManager)
    {
        _cheepService = cheepService;
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
        var user = await _userManager.GetUserAsync(User) ?? throw new UnauthorizedAccessException();
        var (fileBytes, contentType, fileName) = await _cheepService.DownloadAuthorInfo(user.UserName!, user.Email!);
        return File(fileBytes, contentType, fileName);
    }

    public async Task<IActionResult> OnPostForgetMeAsync()
    {
        var Author = await _userManager.GetUserAsync(User);

        await _cheepService.DeleteAuthorByName(Author.UserName);
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Public");
    }
}

