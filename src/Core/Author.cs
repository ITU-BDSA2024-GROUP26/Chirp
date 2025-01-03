using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Core;

/// <summary>
/// The Author class represents our users, inheriting authorization and authentication logic from IdentityUser. 
/// An author can send cheeps and follow other authors. Both of these are 0..N relations
/// </summary>
public class Author : IdentityUser
{
    [Required]
    public ICollection<Cheep>? Cheeps { get; set; }

    // Who this author follows
    [Required]
    public ICollection<Author>? FollowingList { get; set; }

    public bool FollowsAuthor(Author authorToCheck) 
    {
        if(FollowingList == null) { return false; }
        return FollowingList.Contains(authorToCheck); 
    }

    public bool FollowsAuthor(string authorNameToCheck) 
    {
        if(FollowingList == null) { return false; }

        try
        {
            // returns the first element that matches the condition. If no element matches it, throws an InvalidOperationException.
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