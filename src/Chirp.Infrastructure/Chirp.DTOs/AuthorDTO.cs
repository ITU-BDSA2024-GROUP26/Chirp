using Chirp.Core;

namespace Chirp.DTOs;

public class AuthorDTO 
{
    public string Name { get; set; }
    public string Email { get; set; }

    public AuthorDTO(string name, string email) 
    {
        Name = name; 
        Email = email;
    }

    public AuthorDTO(Author author) : this(author.Name, author.Email) {}
}