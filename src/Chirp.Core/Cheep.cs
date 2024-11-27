using System.ComponentModel.DataAnnotations;

namespace Chirp.Core;

public class Cheep
{
    public int Id { get; set; }
    
    [Required]
    public required Author? Author { get; set; }
    
    public string? AuthorId { get; set; }
    
    [Required]
    public required string Text { get; set; }
    
    public DateTime TimeStamp { get; set; }
}