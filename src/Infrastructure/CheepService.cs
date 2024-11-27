using Core;
using Infrastructure;

namespace Infrastructure;

using System.Formats.Tar;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using Core;
using Infrastructure.Migrations;
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


    public async Task SendCheep(string authorName, string content, DateTime timeSent)
    {
        Cheep newCheep = new Cheep
        {
            Author = await cheepRepository.FindAuthorByName(authorName),
            Text = content,
            TimeStamp = timeSent
        };
        await cheepRepository.CreateCheep(newCheep);
        return ;
    }
}