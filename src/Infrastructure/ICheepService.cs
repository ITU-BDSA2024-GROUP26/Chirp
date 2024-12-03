using Core;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;


public interface ICheepService
{
    public Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex = null);
    public Task<IEnumerable<AuthorDto>> GetFollowingAuthorsAsync(string userName);
    public Task<IEnumerable<CheepDTO>> GetFollowingCheepsAsync(int page, string authorName);
    public Task SendCheep(string authorName, string content, DateTime timeSent);
    public Task AddOrRemoveFollower(string userName, string userToFollowName);  
    public Task DeleteAuthorByName(string authorName);
    public Task SeedDatabaseAsync();
    public Task ResetDatabaseAsync();
    public Task<(byte[] FileData, string ContentType, string FileName)> DownloadAuthorInfo(Author author);
}