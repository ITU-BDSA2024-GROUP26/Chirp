
public class CheepDTO 
{
    public int ID {get; set;}
    public string AuthorName { get; set; }
    public string MessageContent { get; set; }
    public string TimeStampStr { get; set; }

    public CheepDTO(string author, string message, DateTime TimeStamp, int id)
    {
        DateTimeOffset timeStampoffset = DateTime.SpecifyKind(TimeStamp, DateTimeKind.Local); 

        TimeStampStr = Chirp.CLI.Client.UserInterface.FormatTimestamp(timeStampoffset.ToUnixTimeSeconds());
        AuthorName = author; 
        MessageContent = message;
        ID = id;
    }

    public CheepDTO(Chirp.Razor.Cheep cheep) : this(cheep.Author.Name, cheep.Text, cheep.TimeStamp, cheep.CheepId) { }
}