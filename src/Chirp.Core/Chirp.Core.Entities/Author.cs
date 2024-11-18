using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Chirp.Core.Entities;

public class Author : IdentityUser
{
    [Required]
    public ICollection<Cheep>? Cheeps { get; set; }

    // Who this author follows
    [Required]
    public ICollection<Author>? FollowingList { get; set; }
}