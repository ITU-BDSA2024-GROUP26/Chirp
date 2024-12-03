using Core;

namespace Infrastructure;

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

public class CheepRepository(CheepDbContext context) : ICheepRepository
{
    public async Task CreateCheep(Cheep newCheep)
    {
        var qwe = await context.AddAsync(newCheep);
         

        // notification logic
        Author sender = newCheep.Author!;

        // for followers
        var followers = from author in context.Users 
                        .Include(a => a.FollowingList)
                        where author.FollowingList != null && author.FollowingList.Contains(sender)
                        select author; 

        await context.SaveChangesAsync();

        foreach(var f in followers) {
            Notification notif = new Notification{ 
                cheepID=qwe.Entity.Id,
                authorID=f.Id,
                tagNotification=false
            };
            await context.AddAsync(notif);
        }

        // for tags TODO 

        await context.SaveChangesAsync();
    }

    public async Task<ICollection<Cheep>> ReadCheeps(int limit = -1, int offset = 0, string? authorNameRegex = null)
    {
        IQueryable<Cheep>? query;

        if (authorNameRegex == null)
        {
            query = (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     orderby cheep.Id descending
                     select cheep)
                    .Skip(offset);
        }
        else
        {
            query = (from cheep in context.Cheeps
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

    public int GetTotalCheepsAccount(string? author = null)
    {
        var query = context.Cheeps.AsQueryable();
        if (!!string.IsNullOrEmpty(author))
        {
            query = query.Where(c => c.Author.UserName == author);
        }
        return query.Count();
    }



    public async Task<ICollection<Cheep>> GetPrivateTimelineCheeps(string userName, int limit = -1, int offset = 0)
    {
        var user = (from u in context.Users
                    .Include(u => u.FollowingList) // need this or nothing works
                    where u.UserName == userName
                    select u).First();

        if (user.FollowingList == null) { return []; }

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
}