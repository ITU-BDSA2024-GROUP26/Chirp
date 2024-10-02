using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor;

public class Cheep
{
    public int CheepId { get; set; }
    public Author Author { get; set; }
    public string Text{ get; set; } 
    public DateTime Timestamp{ get; set; } 
}