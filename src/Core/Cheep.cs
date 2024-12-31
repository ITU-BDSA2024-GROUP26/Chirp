using System.ComponentModel.DataAnnotations;

namespace Core;

/// <summary>
/// The Cheep class represents a cheep in the Chirp application, which is a short message with a sender and a timestamp indicating when it was sent
/// </summary>
public class Cheep
{
    // Note that the EF-core is set up such that this is an autoincremented primary key. 
    public int Id { get; set; }
    
    [Required]
    public required Author? Author { get; set; }
    
    [Required]
    public required string Text { get; set; }
    
    public DateTime TimeStamp { get; set; }
}