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
using Azure.Identity;


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
    public IEnumerable<string> FollowingList { get; set; } = new List<string>();
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
        var followingAuthors = await _cheepRepository.GetAuthorsFollowing(user.UserName);
        FollowingList = followingAuthors.Select(a => a.UserName ?? "Unknown").ToList();

        // Fetch the user's cheeps
        UserCheeps = await _cheepService.GetCheepsAsync(page: 1, authorRegex: user.UserName);

        return Page();
    }

    public async Task<IActionResult> OnPostForgetMeAsync() 
    {
        
       var Author = await _userManager.GetUserAsync(User);

        await _cheepRepository.DeleteAuthorByName(Author.UserName);
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Public");
    }




}

