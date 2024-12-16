using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Core;

public class Author : IdentityUser
{
    [Required]
    public ICollection<Cheep>? Cheeps { get; set; }

    // Who this author follows
    [Required]
    public ICollection<Author>? FollowingList { get; set; }

    public bool FollowsAuthor(string authorNameToCheck) 
    {
        if(FollowingList == null) { return false; }

        try
        {
            // returns the first element that matches the condition. If no file matches it, throws an 
            // InvalidOperationException if no such file exists. A little janky but should work
            FollowingList.First(following => following.UserName == authorNameToCheck);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        // If we reach this code the user must be following a user wiht a matching author name
        return true; 
    }
}   