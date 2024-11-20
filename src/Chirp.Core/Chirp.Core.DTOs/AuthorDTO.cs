using Chirp.Core.Entities;

namespace Chirp.Core.DTOs;

public class AuthorDto(string? userName, string? email, ICollection<string>? Followers)
{
    public string? UserName { get; set; } = userName;
    public string? Email { get; set; } = email;
    public ICollection<string>? FollowingList { get; set; }

    public AuthorDto(Author author) : this(author.UserName, author.Email, ExtractFollowersList(author.FollowingList)) { }

    static private ICollection<string> ExtractFollowersList(ICollection<Author> followersList) 
    {
        ICollection<string> res = new string[followersList.Count]; 

        foreach (Author a in followersList) 
        {
            res.Add(a.UserName ?? throw new Exception("Author with null name")); 
        }

        return res; 
    }
}