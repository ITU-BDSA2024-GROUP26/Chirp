using Core;

namespace Infrastructure;


public interface ICheepService
{
    public Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex = null);
    public Task<IEnumerable<AuthorDto>> GetFollowingAuthorsAsync(string userName);
    public Task<IEnumerable<CheepDTO>> GetFollowingCheepsAsync(int page, string authorName);
    public Task SendCheep(string authorName, string content, DateTime timeSent);
    public Task AddOrRemoveFollower(string userName, string userToFollowName);  
    public Task DeleteAuthorByName(string authorName); 
    public Task<IEnumerable<NotificationDTO>> GetAuthorsNotifications(string userName, bool getOld); 
}