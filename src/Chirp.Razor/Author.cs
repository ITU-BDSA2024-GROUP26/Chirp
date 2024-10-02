namespace Chirp.Razor;

public class Author
{
    private string name { get; set; }
    private string emailAddress { get; set; }
    private ICollection<Cheep> cheeps { get; set; }
    

}