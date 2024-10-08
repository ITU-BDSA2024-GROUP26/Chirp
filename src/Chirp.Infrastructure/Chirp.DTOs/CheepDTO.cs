using System.Globalization;
using Chirp.Core;

namespace Chirp.DTOs;

public class CheepDTO 
{
    public int ID {get; set;}
    public string AuthorName { get; set; }
    public string MessageContent { get; set; }
    public string TimeStampStr { get; set; }

    public CheepDTO(string author, string message, DateTime timeStamp, int id)
    {
        TimeStampStr = timeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture);
        AuthorName = author; 
        MessageContent = message;
        ID = id;
    }

    public CheepDTO(Cheep cheep) : this(cheep.Author.Name, cheep.Text, cheep.TimeStamp, cheep.CheepId) { }
}