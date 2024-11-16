using Chirp.Core.Entities;

namespace Chirp.Core.DTOs;

public class AuthorDto(string? userName, string? email)
{
    public string? UserName { get; set; } = userName;
    public string? Email { get; set; } = email;

    public AuthorDto(Author author) : this(author.UserName, author.Email) { }
}