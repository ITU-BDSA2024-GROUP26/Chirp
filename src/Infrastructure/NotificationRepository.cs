using System.Text.RegularExpressions;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure;

public class NotificationRepository(CheepDbContext context) : INotificationRepository
{
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
    }

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