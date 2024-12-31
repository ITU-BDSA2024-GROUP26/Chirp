using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;
using System.Linq;
using Core;

/// <summary>
/// CheepService(Which probably should have been called ChirpService, since it serves the entire domain) is responsible for handling communication between the presentation/web layer and the repositories
/// In addition, it is responsible for ecapsulating the actual Core objects in DTOs
/// </summary>
/// <param name="cheepRepository">ICheepRepository implementation, injected via DI</param>
/// <param name="authorRepository">IAuthorRepository implementation, injected via DI</param>
/// <param name="dbRepository">IDbRepository implementation, injected via DI</param>
/// <param name="notificationRepository">INotificationRepository implementation, injected via DI</param>
public class CheepService(
    ICheepRepository cheepRepository, 
    IAuthorRepository authorRepository,
    IDbRepository dbRepository, 
    INotificationRepository notificationRepository) : ICheepService
{
    // Perhaps this should be configurable, but in keeping with KISS this is an extremely simple way to implement that functional requirement. 
    private const int MaxCheeps = 32;

    // Query for cheeps, including encapsulating every Cheep into a CheepDTO 
    // - page: Perhaps too specific to the presentation layer for something in the Infrastructure layer, this is based on the logic that every page has MaxCheeps cheeps
    // - authorRegex: Misnamed due to a different earlier implementation, simply if this isn't null we only return cheeps by that author. 
    public async Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex = null)
    {
        var cheeps = await cheepRepository.ReadCheeps(MaxCheeps, (page - 1) * MaxCheeps, authorRegex);
        return cheeps.Select(cheep => new CheepDTO(cheep));
    }

    // Query for all the cheeps from the people authorName follows. 
    // - page: Perhaps too specific to the presentation layer for something in the Infrastructure layer, this is based on the logic that every page has MaxCheeps cheeps
    // - authorName: The username(primary key) of the author who we want to display these cheeps for. I.e. we get the cheeps of everyone he follows
    public async Task<IEnumerable<CheepDTO>> GetFollowingCheepsAsync(int page, string authorName)
    {
        var cheeps = await cheepRepository.GetPrivateTimelineCheeps(authorName, MaxCheeps, MaxCheeps * (page - 1));
        return cheeps.Select(cheep => new CheepDTO(cheep));
    }

    // Query for all the authors userName follows 
    public async Task<IEnumerable<AuthorDto>> GetFollowingAuthorsAsync(string userName)
    {
        var followers = await authorRepository.GetAuthorsFollowing(userName);

        return followers.Select(author => new AuthorDto(author));
    }

    // Command for the Author identified by *authorName* to send a cheep with *content* at time *timesent* 
    public async Task SendCheep(string authorName, string content, DateTime timeSent)
    {
        Cheep newCheep = new Cheep
        {
            Author = await authorRepository.FindAuthorByName(authorName),
            Text = content,
            TimeStamp = timeSent
        };
        await cheepRepository.CreateCheep(newCheep);
        return ;
    }

    // Command to add *userName* as a follower of *userToFollowName* if not following, otherwise remove him  
    public async Task AddOrRemoveFollower(string userName, string userToFollowName) 
    {
        await authorRepository.AddOrRemoveFollower(userName, userToFollowName); 
        return; 
    }

    // Command to delete the author identified by *authorName*. Note this also implies the deletion of all the cheeps he ever sent(and notifications addressed to him )
    public async Task DeleteAuthorByName(string authorName) 
    {
        await authorRepository.DeleteAuthorByName(authorName); 
    }

    // Query to get all the notifications targeted at *userName*(i.e. the cheeps of the Authors he follows and the cheeps he is tagged in).
    // *getOld* refers to whether we should retrieve notification that we have already retrieved once before or not.  
    public async Task<IEnumerable<NotificationDTO>> GetAuthorsNotifications(string userName, bool getOld)
    { 
        var notifs = await notificationRepository.GetNotifications(userName, getOld);
        List<NotificationDTO> retList = new List<NotificationDTO>(notifs.Count); 

        foreach(var notif in notifs) {
            retList.Add(new NotificationDTO(notif.cheep.Text, notif.cheep.Author!.UserName!, notif.tagNotification)); 
        }
        
        return retList;
    }

    // A command to seed the database with the initial data(a functional requirement)
    public async Task SeedDatabaseAsync()
    {
        await dbRepository.SeedAsync();
    }

    // A command to reset the database to the default(empty) state
    public async Task ResetDatabaseAsync()
    {
        await dbRepository.ResetAsync();
    }

    // A query for the data of the author, encoded as a string of bytes. Very implementation specific to an HTTP file response which is probably not ideal for the Infrastructure layer. 
    // On the other hand, in keeping with the KISS principle, it greatly simplifies the usage of the query.  
    public async Task<(byte[] FileData, string ContentType, string FileName)> DownloadAuthorInfo(string name, string email)
    {
        var followingList = await GetFollowingAuthorsAsync(name!);
        var userCheeps = await cheepRepository.ReadCheeps(-1, 0, name);
        
        // Create the textfile
        var content = new StringBuilder();
        content.AppendLine($"{name}'s information:");
        content.AppendLine($"-----------------------");
        content.AppendLine($"Name: {name}");
        content.AppendLine($"Email: {email}");
        
        content.AppendLine("Following:");
        if (followingList.Any())
        {
            foreach (var following in followingList)
            {
                content.AppendLine($"- {following.UserName}");
            }
        }
        else content.AppendLine("- No following");
        
        content.AppendLine("Cheeps:");
        if (userCheeps.Count != 0)
        {
            foreach (var cheep in userCheeps)
            {
                var formattedTimestamp = cheep.TimeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture);
                content.AppendLine($"- \"{cheep.Text}\" ({formattedTimestamp})");
            }
        }
        else content.AppendLine("- No Cheeps posted yet");

        // Convert content into bytes and return file
        var fileBytes = Encoding.UTF8.GetBytes(content.ToString());
        const string contentType = "text/plain";
        var fileName = $"{name}_Chirp_data.txt";
        return (fileBytes, contentType, fileName);
    }
}
