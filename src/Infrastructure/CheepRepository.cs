using Core;

namespace Infrastructure;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Class that allows us to interface with the Cheep portion of our database. 
/// </summary>
/// <param name="context">The EF-Core database context to interact with</param>
/// <param name="notificationRepository">The NotificationRepository. CheepRepository depends on this because it's responsible for adding cheeps, at which time notifications should also be created</param>
public class CheepRepository(CheepDbContext context, INotificationRepository notificationRepository) : ICheepRepository
{
    // Command to create a cheep, expects you to have filled out a Cheep object with the required attributes(Text and Author) as well as optionally timestamp. 
    // The database will itself create a unique ID, since cheepID is an autoincremented primary key
    public async Task CreateCheep(Cheep newCheep)
    {
        var tracker = await context.AddAsync(newCheep);

        // need to save changes to ensure that the tracker knows the ID of the cheep
        await context.SaveChangesAsync();
        
        await notificationRepository.CreateNotification(tracker);
    }

    // Query cheeps from the database, with the following parameters 
    // - limit: How many cheeps should we return at most? If <= 0 we return all matching cheeps
    // - offset: Skips the first x amount of cheeps. Note that this can be negative, which might cause problems. 
    // - authorNameRegex: outdated name, instead only cheaps by the author whose username *strictly* matches will be returned. If null, return cheeps from all authors
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

    // Simple command that changes the message of the cheep with the given id 
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

    // Query that returns how many cheeps this author has made in total. 
    public int GetTotalCheepsAccount(string? author = null)
    {
        var query = context.Cheeps.AsQueryable();
        if (!!string.IsNullOrEmpty(author))
        {
            query = query.Where(c => c.Author.UserName == author);
        }
        return query.Count();
    }

    // Query for the private timeline cheeps of an author, which are the cheeps of everyone he follows, the cheeps he himself has made and the cheeps where he is tagged
    // The arguments are: 
    // - limit: How many cheeps should we return at most? If <= 0 we return all matching cheeps
    // - offset: Skips the first x amount of cheeps. Note that this can be negative, which might cause problems. 
    // - userName: The username of the author whose private timeline we want to get. Note the query will fail if this doesn't match an existing user, which is probably desirable(Higher levels should validate this, we want an exception thrown so we can fix it if some bug means we pass invalid usernames here)
    public async Task<ICollection<Cheep>> GetPrivateTimelineCheeps(string userName, int limit = -1, int offset = 0)
    {
        var user = (from u in context.Users
                    .Include(u => u.FollowingList) // need this or nothing works
                    where u.UserName == userName
                    select u).First();

        
        var notifQuery = from notif in context.notifications 
                        where notif.authorID == user.Id
                        select notif.cheepID; 
        IQueryable<Cheep> query; 
        if (user.FollowingList == null) { 
            query = (from cheep in context.Cheeps
                        .Include(c => c.Author) // from chatgpt 
                        where  cheep.Author!.UserName == userName || notifQuery.Contains(cheep.Id)
                        orderby cheep.Id descending
                        select cheep)
                        .Skip(offset);
        } else {
            query = (from cheep in context.Cheeps
                        .Include(c => c.Author) // from chatgpt 
                        where user.FollowingList.Contains(cheep.Author!) || cheep.Author!.UserName == userName 
                        || notifQuery.Contains(cheep.Id)
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
}