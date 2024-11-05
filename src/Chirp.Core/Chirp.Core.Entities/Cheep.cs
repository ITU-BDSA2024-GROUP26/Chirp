using System.ComponentModel.DataAnnotations;

namespace Chirp.Core.Entities;

public class Cheep
{
    public int Id { get; set; }
    
    [Required]
    public required Author Author { get; set; }
    public int AuthorId { get; set; }
    
    [Required]
    public required string Text { get; set; }
    
    public DateTime TimeStamp { get; set; }
}