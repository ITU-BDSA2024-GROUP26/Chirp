using System.ComponentModel.DataAnnotations;

namespace Chirp.Razor;

public class Cheep
{
    private Author author { get; set; }
    private string text{ get; set; } 
    private DateTime TimestampAttribute{ get; set; } 
}