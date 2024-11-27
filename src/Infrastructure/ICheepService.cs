using Core;

namespace Infrastructure;

using Core;

public interface ICheepService
{
    public Task<IEnumerable<CheepDTO>> GetCheepsAsync(int page, string? authorRegex = null);
    public Task<IEnumerable<AuthorDto>> GetFollowingAuthorsAsync(string userName);
    public Task<IEnumerable<CheepDTO>> GetFollowingCheepsAsync(int page, string authorName);
}