
public class CheepDTO 
{
    public string AuthorName { get; set; }
    public string MessageContent { get; set; }
    public string TimeStampStr { get; set; }

    public CheepDTO(string author, string message, long TimeStamp)
    {
        TimeStampStr = Chirp.CLI.Client.UserInterface.FormatTimestamp(TimeStamp);
        AuthorName = author; 
        MessageContent = message;
    }

    public CheepDTO(Cheep cheep) : this(cheep.Author, cheep.Message, cheep.Timestamp) {}
}