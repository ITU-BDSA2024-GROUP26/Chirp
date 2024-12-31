using System.Text.RegularExpressions;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure;

/// <summary>
/// A class to interface with the notification part of the database. 
/// </summary>
/// <param name="context">The EF-Core database context to interface with</param>
public class NotificationRepository(CheepDbContext context) : INotificationRepository
{
    // Command to create a new notification. 
    // Note that this is passed an EntityEntry since cheepID is an autoincrement primary key, and thus isn't known untill the cheep has been inserted in the database. 
    // An EntityEntry is a copy of the actual cheep in the database, which can be returned after the command to insert the cheap returns successfully. 
    public async Task CreateNotification(EntityEntry<Cheep> cheepTracker)
    {
        Author sender = cheepTracker.Entity.Author ?? throw new Exception("Sender null when trying to create notification");
        // for followers
        var followers = from author in context.Users 
                        .Include(a => a.FollowingList)
                        where author.FollowingList != null && author.FollowingList.Contains(sender)
                        select author;
        
        // for tags. NOTE: Tags are prioritized over following notifications 
        var taggedAuthors = new List<Author>(); 
        var tagFinderRegex = new Regex("@(\\w+)"); 
        var matches = tagFinderRegex.Matches(cheepTracker.Entity.Text); 
        
        foreach (Match match in matches) {
            if (match.Groups.Count < 2) { continue; }
            var authorToTag = await context.Users.FirstOrDefaultAsync(a => a.UserName == match.Groups[1].Value); 
            if (authorToTag == null) { continue; } // don't require input validation in higher levels 
            taggedAuthors.Add(authorToTag); // to make sure we don't double notify
            
            Notification notif = new Notification{ 
                cheepID=cheepTracker.Entity.Id,
                authorID=authorToTag!.Id,
                tagNotification=true,
                isNew=true
            };
            await context.AddAsync(notif);
        }

        foreach(var f in followers) {
            if(taggedAuthors.Contains(f)) { continue; }
            
            Notification notif = new Notification{ 
                cheepID=cheepTracker.Entity.Id,
                authorID=f.Id,
                tagNotification=false,
                isNew=true
            };
            await context.AddAsync(notif);
        }

        await context.SaveChangesAsync();
    }

    // Simple query to get all the notifications addressed to *authorName*, with *getOld* indicating whether we should refetch old notifications, i.e. notifications that have already been fetched once before. 
    // Note that we also set every notification fetched here to be "old"
    public async Task<ICollection<Notification>> GetNotifications(string authorName, bool getOld) 
    {
        var author = await context.Users.FirstOrDefaultAsync(a => a.UserName == authorName); 
        var query = from notif in context.notifications 
                    .Include(n => n.cheep).ThenInclude(c => c.Author)
                    where notif.authorID == author!.Id && (notif.isNew || getOld)
                    select notif; 
        

        var retList = await query.ToListAsync();
        await query.ForEachAsync(n => n.isNew = false); 

        await context.SaveChangesAsync(); 

        return retList; 
    }
}