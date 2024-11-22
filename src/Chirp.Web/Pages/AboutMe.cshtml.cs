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


namespace Chirp.Razor.Pages;

[Authorize]
public class AboutMeModel : PageModel
{
    private readonly ICheepService _cheepService;
    private readonly ICheepRepository _cheepRepository;
    private readonly UserManager<Author> _userManager;



    public AboutMeModel(ICheepService cheepService, ICheepRepository cheepRepository, UserManager<Author> userManager)
    {
        _cheepService = cheepService;
        _cheepRepository = cheepRepository;
        _userManager = userManager;
    }

    // Properties to hold user data 
    public AuthorDto CurrentUser { get; set; }
    public IEnumerable<string> FollowingList { get; set; }
    public IEnumerable<CheepDTO> UserCheeps { get; set; }

    // Handler for GET request
    public async Task<IActionResult> OnGetAsync()
    {
        // Retrieve the current user
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        // Map to DTO
        CurrentUser = new AuthorDto(user);

        // Fetch the list of users the current user is following 
        var followingAuthors = await _cheepRepository.GetAuthorsFollowing(user.UserName);
        FollowingList = followingAuthors.Select(a => a.UserName ?? "Unknown");

        // Fetch the users cheeps
        UserCheeps = await _cheepService.GetCheepsAsync(page: 1, authorRegex: user.UserName);

        return Page();

    }



}



