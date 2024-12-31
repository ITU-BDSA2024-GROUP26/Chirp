using Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

/// <summary>
/// A repository class which enables queries and commands to the Authors table
/// </summary>
/// <param name="context">The EF-Core database context this repository interacts with, injected via DI</param>
public class AuthorRepository(CheepDbContext context) : IAuthorRepository
{
    // Simple query to get author by name
    public async Task<Author?> FindAuthorByName(string name)
    {
        return await context.Users.FirstOrDefaultAsync(a => a.UserName == name);
    }
    // Simple query to get author by email. We needed this for the first implementation of the author class where the primary key was the email, and saw no reason to remove the functionality from the repository
    public async Task<Author?> FindAuthorByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(a => a.Email == email);
    }
    // Query that, given the primary key(username) of an author returns a list of everyone he follows
    public async Task<ICollection<Author>> GetAuthorsFollowing(string name)
    {
        Author author = await
            (from a in context.Users
            .Include(a => a.FollowingList) /* need this or nothing works */
            where a.UserName == name
            select a).FirstAsync();
        

        return author.FollowingList ?? new List<Author>();
    }

    // Command which adds the follower, if user is already following unfollow instead
    public async Task AddOrRemoveFollower(string userName, string usernmToFollow)
    {
        if (userName == usernmToFollow) { throw new Exception("User can't follow himself"); }

        Author userTofollow = await FindAuthorByName(usernmToFollow) ?? throw new Exception($"Null user to follow {usernmToFollow}");

        var user =
            (from a in context.Users
            .Include(a => a.FollowingList)
            where a.UserName == userName
            select a).First();

        user.FollowingList ??= new List<Author>();

        if (user.FollowingList.Contains(userTofollow))
        {
            user.FollowingList.Remove(userTofollow);
        }
        else
        {
            user.FollowingList.Add(userTofollow);
        }

        await context.SaveChangesAsync();
    }

    // Command that removes the author via identified by his username
    public async Task<Author?> DeleteAuthorByName(string name)
    {
        // Find author by name
        var user = await context.Users
                       .Include(a => a.Cheeps) // Ensure the user's cheep
                       .Include(a => a.FollowingList)
                       .FirstOrDefaultAsync(a => a.UserName == name)
                   ?? throw new Exception("User cannot be found!");
        
        // remove the user we want to delete from the followinglists of all his followers, to avoid "danging" foreign keys
        var followers = from u in context.Users
                        .Include(a => a.FollowingList)
                        where u.FollowingList != null && u.FollowingList!.Contains(user)
                        select u; 
        
        await followers.ForEachAsync(u => {
            u.FollowingList!.Remove(user);
        }); 


        // Remove user's cheeps
        if (user.Cheeps != null && user.Cheeps.Any())
        {
            context.Cheeps.RemoveRange(user.Cheeps);
        }

        // Remove user
        context.Users.Remove(user);

        // Save changes
        await context.SaveChangesAsync();

        return user;
    }
}