using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Repositories;

using Core;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

public class CheepRepository(CheepDbContext context) : ICheepRepository
{
    public async Task CreateCheep(Cheep newCheep)
    {
        await context.AddAsync(newCheep);
        await context.SaveChangesAsync(); 
    }
  
    public async Task<Author?> FindAuthorByName(string name)
    {
        return await context.Users.FirstOrDefaultAsync(a=> a.UserName == name);
    } 

    public async Task<Author?> FindAuthorByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(a=> a.Email == email);
    } 

    public async Task<ICollection<Cheep>> ReadCheeps(int limit = -1, int offset = 0, string? authorNameRegex = null)
    {
        IQueryable<Cheep>? query;
            
        if(authorNameRegex == null) 
        {
            query=  (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);
        } else {
            query =  (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where cheep.Author!.UserName! == authorNameRegex!
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);
        } 

        if (limit > 0)
        {
            query = query.Take(limit);
        }

        await context.SaveChangesAsync();

        return await query.ToListAsync();
    }

    public async Task UpdateCheep(int id, string newMessage)
    {
        var query =
                    from cheep in context.Cheeps
                    where cheep.Id == id
                    select cheep;

        // Possible todo: record the update timestamp
        // https://stackoverflow.com/questions/20832684/update-records-using-linq 
        await query.ForEachAsync(cheep => cheep.Text = newMessage);

        await context.SaveChangesAsync();
    }

    public async Task<ICollection<Author>> GetAuthorsFollowing(string name) 
    {
        Author author = await FindAuthorByName(name) ?? throw new Exception($"Null author {name}");

        IQueryable<Author> query = 
            from a in context.Users 
            where author.FollowingList.Contains(a) // this will only work if there is some inbuilt equality shit 
            select a; 
        
        await context.SaveChangesAsync(); 
        return await query.ToListAsync(); 
    }

    // Adds the follower, if user is already following unfollow instead
    public async Task AddOrRemoveFollower(string userName, string usernmToFollow)
    {
        if(userName == usernmToFollow) { throw new Exception("User can't follow himself"); }

        Author userTofollow = await FindAuthorByName(usernmToFollow) ?? throw new Exception($"Null user to follow {usernmToFollow}");
        
        var query =
            from a in context.Users
            .Include(a => a.FollowingList)
            where a.UserName == userName 
            select a; 
        
        await query.ForEachAsync(user => {
            if(user.FollowingList == null) { Console.WriteLine("Fucking followinglist is null"); }
            user.FollowingList ??= new List<Author>();

            if(user.FollowingList.Contains(userTofollow)) {
                user.FollowingList.Remove(userTofollow);
            } else {
                user.FollowingList.Add(userTofollow);
            }
            });
    
        await context.SaveChangesAsync(); 
    }

    public async Task<ICollection<Cheep>> GetPrivateTimelineCheeps(string userName, int limit = -1, int offset = 0)
    {   
        var user = (from u in context.Users
                    .Include(u => u.FollowingList) // need this or nothing works
                    where u.UserName == userName
                    select u).First(); 

        if(user.FollowingList == null) { return []; }

        var query = (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where user.FollowingList.Contains(cheep.Author) || cheep.Author.UserName == userName
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);

        if (limit > 0)
        {
            query = query.Take(limit);
        }
        await context.SaveChangesAsync();

        return await query.ToListAsync();
    } 

     public async Task<Author?> DeleteAuthorByName(string name)
    {
        var user = await context.Users.FirstOrDefaultAsync(a=> a.UserName == name) ?? throw new Exception("User can not be found!");

        context.Users.Remove(user);
        //check whether or not the deleted user's cheeps also are deleted
        //if not then add manually smthn like .Include(cheeps???) 
        await context.SaveChangesAsync(); 
        return user;  
    }

    public async Task<Author?> DeleteAuthor(string name)
    {
        // Find author by name
        var user = await context.Users
                       .Include(a => a.Cheeps) // Ensure the user's cheep
                       .FirstOrDefaultAsync(a => a.UserName == name) 
                   ?? throw new Exception("User cannot be found!");

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