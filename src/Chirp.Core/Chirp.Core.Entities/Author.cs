using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Entities;

public class Author
{
    public int AuthorId { get; set; }
    
    [Required]
    public required string Name { get; set; }
    public required string Email { get; set; }
    
    [Required]
    public required ICollection<Cheep> Cheeps { get; set; }


}