namespace Core;

public class AuthorDto(string? userName, string? email, ICollection<AuthorDto>? FollowingList)
{
    public string? UserName { get; set; } = userName;
    public string? Email { get; set; } = email;
    public ICollection<AuthorDto>? FollowingList { get; set; }

    public AuthorDto(Author author) : this(author.UserName, author.Email, ExtractFollowersList(author.FollowingList)) { }

    static private ICollection<AuthorDto> ExtractFollowersList(IEnumerable<Author>? followersList)
    {
        if (followersList == null)
        {
            return new List<AuthorDto>();
        }

        return followersList.Select(author => new AuthorDto(author.UserName, author.Email, null)).ToList();
    }
}