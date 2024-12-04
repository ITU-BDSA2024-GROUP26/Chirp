using Core;

namespace Infrastructure;

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
using SQLitePCL;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Infrastructure.Migrations;

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

        // need to save changes for us to find out what the ID of the cheep will be 
        await context.SaveChangesAsync();

        foreach(var f in followers) {
            Notification notif = new Notification{ 
                cheepID=qwe.Entity.Id,
                authorID=f.Id,
                tagNotification=false,
                isNew=true
            };
            await context.AddAsync(notif);
        }

        // for tags TODO 
        var tagFinderRegex = new Regex("@(\\w+)"); 
        var matches = tagFinderRegex.Matches(newCheep.Text); 
        foreach (Match match in matches) {
            if (match.Groups.Count < 2) { continue; }
            var authorToTag = await context.Users.FirstOrDefaultAsync(a => a.UserName == match.Groups[1].Value); 
            if (authorToTag == null) { continue; } // don't require input validation in higher levels 
            Notification notif = new Notification{ 
                cheepID=qwe.Entity.Id,
                authorID=authorToTag!.Id,
                tagNotification=true,
                isNew=true
            };
            await context.AddAsync(notif);
        }

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

        var notifQuery = from notif in context.notifications 
                        where notif.authorID == user.Id
                        select notif.cheepID; 

        var query = (from cheep in context.Cheeps
                    .Include(c => c.Author) // from chatgpt 
                     where user.FollowingList.Contains(cheep.Author!) || cheep.Author!.UserName == userName 
                     || notifQuery.Contains(cheep.Id)
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