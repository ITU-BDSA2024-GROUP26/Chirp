using System.Globalization;

namespace Core;

public class CheepDTO(string author, string message, DateTime timeStamp, int id)
{
    public int Id { get; set; } = id;
    public string AuthorName { get; set; } = author;
    public string MessageContent { get; set; } = message;
    public string TimeStampStr { get; set; } = timeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture);

    public CheepDTO(Cheep cheep) : this(
        cheep.Author?.UserName ?? throw new ArgumentNullException(nameof(cheep.Author), "Author cannot be null."),
        cheep.Text ?? throw new ArgumentNullException(nameof(cheep.Text), "Cheep text cannot be null."),
        cheep.TimeStamp,
        cheep.Id)
    { }
}