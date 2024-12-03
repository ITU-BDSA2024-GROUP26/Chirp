namespace Infrastructure;
using System.Linq;
using Core;

public class CheepService(ICheepRepository cheepRepository, IAuthorRepository authorRepository) : ICheepService
{
    private const int MaxCheeps = 32;

    public async Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex = null)
    {
        var cheeps = await cheepRepository.ReadCheeps(MaxCheeps, (page - 1) * MaxCheeps, authorRegex);
        return cheeps.Select(cheep => new CheepDTO(cheep));
    }

    public async Task<IEnumerable<CheepDTO>> GetFollowingCheepsAsync(int page, string authorName)
    {
        var cheeps = await cheepRepository.GetPrivateTimelineCheeps(authorName, MaxCheeps, MaxCheeps * (page - 1));
        return cheeps.Select(cheep => new CheepDTO(cheep));
    }

    public async Task<IEnumerable<AuthorDto>> GetFollowingAuthorsAsync(string userName)
    {
        var followers = await authorRepository.GetAuthorsFollowing(userName);

        return followers.Select(author => new AuthorDto(author));
    }


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

    public async Task AddOrRemoveFollower(string userName, string userToFollowName) 
    {
        await authorRepository.AddOrRemoveFollower(userName, userToFollowName); 
        return; 
    }

    public async Task DeleteAuthorByName(string authorName) 
    {
        await authorRepository.DeleteAuthorByName(authorName); 
    }

    public async Task<IEnumerable<NotificationDTO>> GetAuthorsNotifications(string userName)
    { 
        var notifs = await authorRepository.GetNotifications(userName);

        List<NotificationDTO> retList = new List<NotificationDTO>(notifs.Count); 

        foreach(var notif in notifs) {
            retList.Add(new NotificationDTO(notif)); 
        }
        
        return retList;
    }
}
