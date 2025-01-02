namespace Core;

/// <summary>
/// The AuthorDTO class encapsulates the information that the frontend(the razor pages) need to know about an Author. 
/// Specifically, this doesn't include any authorization or authentication information. 
/// In addition to this, all the types are reduced to either primitive types or references to other DTOs. 
/// </summary>
/// <param name="userName">The username of the author, which is also the primary key of the corresponding Author object</param>
/// <param name="email">The email address of the author, which isn't really used for anything</param>
/// <param name="FollowingList">List of other DTOs who this author follows</param>
public class AuthorDto(string? userName, string? email, ICollection<AuthorDto>? FollowingList)
{
    public string? UserName { get; set; } = userName;
    public string? Email { get; set; } = email;
    public ICollection<AuthorDto>? FollowingList { get; set; } = FollowingList;
    // The stack of notifications that this author should be shown. Note that we use a stack here because an author should be notified of the most recent notifications first, for which a Stack fits perfectly
    public Stack<NotificationDTO>? notifications { get; set; }
    // An alternative constructor that lets us convert an Author to an AuthorDTO easily
    public AuthorDto(Author author) : this(author.UserName, author.Email, ExtractFollowersList(author.FollowingList)) { }
    // A utility function that converts the following-list of the Author, which contains references to other Authors, to AuthorDTO objects 
    static private ICollection<AuthorDto> ExtractFollowersList(IEnumerable<Author>? followersList)
    {
        // Ensure that the followingList isn't null
        if (followersList == null)
        {
            return new List<AuthorDto>();
        }

        // We map each author to a new AuthorDTO object. One could raise concerns about overusing space, but this is unlikely to be a concern since only a small subset of the authors should be converted to DTOs and displayed at any given time
        return followersList.Select(author => new AuthorDto(author.UserName, author.Email, null)).ToList();
    }
}