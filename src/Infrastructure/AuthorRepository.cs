using Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class AuthorRepository(CheepDbContext context) : IAuthorRepository
{
    public async Task<Author?> FindAuthorByName(string name)
    {
        return await context.Users.FirstOrDefaultAsync(a => a.UserName == name);
    }

    public async Task<Author?> FindAuthorByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<ICollection<Author>> GetAuthorsFollowing(string name)
    {
        //Author author = await FindAuthorByName(name) ?? throw new Exception($"Null author {name}");

        Author author = await
            (from a in context.Users
            .Include(a => a.FollowingList) /* need this or nothing works */
            where a.UserName == name
            select a).FirstAsync();

        if(author.FollowingList != null && author.FollowingList.Contains(author)) { throw new Exception("Author follows himself"); }

        return author.FollowingList ?? new List<Author>();
    }

    // Adds the follower, if user is already following unfollow instead
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

    public async Task<ICollection<Notification>> GetNotifications(string authorName, bool getOld)
    {
        var author = await FindAuthorByName(authorName); 
        var query = from notif in context.notifications 
                    .Include(n => n.cheep).ThenInclude(c => c.Author)
                    where notif.authorID == author!.Id && (notif.isNew || getOld)
                    select notif; 
        
        Console.WriteLine($"Notifications length in repository {query.Count()}");

        var retList = await query.ToListAsync();

        await query.ForEachAsync(n => n.isNew = false); 

        await context.SaveChangesAsync(); 


        return retList; 
    }
}