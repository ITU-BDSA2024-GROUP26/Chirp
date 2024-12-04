using Microsoft.AspNetCore.Mvc.RazorPages;
using Core;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Infrastructure.Migrations;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.AspNetCore.Authentication;
using System.Text;

namespace Web.Pages;

[Authorize]
public class ChangePageModel : PageModel
{
    private readonly ICheepService _cheepService; 
    private readonly UserManager<Author> _userManager;

    public ChangePageModel(ICheepService cheepService, UserManager<Author> userManager)
    {
        _cheepService = cheepService;
        _userManager = userManager;
    }

    public int CurrentPage { get; set; } = 1;

    public int MaxCheeps { get; set; } = 32; 

    public int TotalCheeps {get; set;} 

    public IEnumerable<CheepDTO> Cheeps { get; set; } = new List<CheepDTO>();
    [Required]
    public string Direction { get; set; }


    public async Task<IActionResult> OnGetChangePageAsync(string Direction, int currentPage = 1) {
        

        TotalCheeps = _cheepService.GetTotalCheepsAccount();
       
        if (Direction == "Previous" && currentPage > 1) 
        {
            CurrentPage = currentPage - 1;  // go to previous page
        } 
        else if(Direction == "Next" && currentPage * MaxCheeps < TotalCheeps)
        {
             CurrentPage = currentPage + 1; // go to next page
        } 
        else
        {
            CurrentPage = currentPage; // stay on the same page
        }
       
    
        Cheeps = await _cheepService.GetCheepsAsync(CurrentPage); 

        return Page();

    }
}
