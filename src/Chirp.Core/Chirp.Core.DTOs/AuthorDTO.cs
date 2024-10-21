using Chirp.Core.Entities;

namespace Chirp.Core.DTOs;

public class AuthorDTO(string name, string email)
{
    public string Name { get; set; } = name;
    public string Email { get; set; } = email;

    public AuthorDTO(Author author) : this(
        author.Name ?? throw new ArgumentNullException(nameof(author.Name), "Author cannot be null"),
        author.Email ?? throw new ArgumentNullException(nameof(author.Email), "Email cannot be null"))
    { }
}