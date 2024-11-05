using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Entities;

public class Author : IdentityUser
{
    [Required]
    public required string Name { get; init; }
    
    [Required]
    public required ICollection<Cheep> Cheeps { get; set; }
}