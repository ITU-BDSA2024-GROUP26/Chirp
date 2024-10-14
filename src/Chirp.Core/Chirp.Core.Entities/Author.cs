namespace Chirp.Core.Entities;
using System.ComponentModel.DataAnnotations;

public class Author
{
    public int AuthorId { get; set; }
    
    [Required] 
    public string Name { get; set; }
    public string Email { get; set; }
    public ICollection<Cheep> Cheeps { get; set; }
    

}