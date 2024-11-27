using Chirp.Core;
using Chirp.Infrastructure;

namespace Chirp.Infrastructure;

using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Chirp.Core;
using Chirp.Infrastructure.Migrations;
using Microsoft.AspNetCore.Identity;

public class CheepService(ICheepRepository cheepRepository) : ICheepService
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
        var followers = await cheepRepository.GetAuthorsFollowing(userName);

        return followers.Select(author => new AuthorDto(author));
    }


    /*public async Task<IEnumerable<Author>?> ExtractFollowersList(Author.followersList)
    {
        if (followersList == null)
        {
            return new string[0];
        }

        ICollection<string> res = new string[followersList.Count];

        foreach (Author a in followersList)
        {
            res.Add(a.UserName ?? throw new Exception("Author with null name"));
        }

        return res;
    }*/
}
