using Core;

namespace Infrastructure;

using Microsoft.EntityFrameworkCore;

public class CheepRepository(CheepDbContext context, INotificationRepository notificationRepository) : ICheepRepository
{
    public async Task CreateCheep(Cheep newCheep)
    {
        var tracker = await context.AddAsync(newCheep);

        // need to save changes to ensure that the tracker knows the ID of the cheep
        await context.SaveChangesAsync();
        
        await notificationRepository.CreateNotification(tracker);
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