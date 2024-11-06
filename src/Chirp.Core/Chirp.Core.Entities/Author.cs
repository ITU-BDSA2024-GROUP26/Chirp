using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Entities;

public class Author : IdentityUser
{
    [Required]
    public string? Name { get; set; }
    
    [Required]
    public ICollection<Cheep>? Cheeps { get; set; }
}