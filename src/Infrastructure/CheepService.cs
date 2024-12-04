using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure;
using System.Linq;
using Core;

public class CheepService(
    ICheepRepository cheepRepository, 
    IAuthorRepository authorRepository,
    IDbRepository dbRepository) : ICheepService
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

    public async Task SeedDatabaseAsync()
    {
        await dbRepository.SeedAsync();
    }

    public async Task ResetDatabaseAsync()
    {
        await dbRepository.ResetAsync();
    }

    public async Task<(byte[] FileData, string ContentType, string FileName)> DownloadAuthorInfo(Author author)
    {
        var name = author.UserName;
        var email = author.Email;
        
        // var followingList = await GetFollowingAuthorsAsync(name);
        // var userCheeps = await cheepRepository.ReadCheeps(-1, 0, );
        
        // Create the textfile
        var content = new StringBuilder();
        content.AppendLine($"{name}'s information:");
        content.AppendLine($"-----------------------");
        content.AppendLine($"Name: {name}");
        content.AppendLine($"Email: {email}");
        
        content.AppendLine("Following:");
        if (author.FollowingList != null && author.FollowingList.Count != 0)
        {
            foreach (var following in author.FollowingList)
            {
                content.AppendLine($"- {following.UserName}");
            }
        }
        else content.AppendLine("- No following");
        
        content.AppendLine("Cheeps:");
        if (author.Cheeps != null && author.Cheeps.Count != 0)
        {
            foreach (var cheep in author.Cheeps)
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
