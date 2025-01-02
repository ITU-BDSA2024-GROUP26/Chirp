using System.Globalization;

namespace Core;

/// <summary>
/// CheepDTO encapsulates the information that we need to display to users about a particular cheep, also reducing the member variables to simple types
/// </summary>
/// <param name="author">Author who wrote the cheep</param>
/// <param name="message">The actual message content of the cheep</param>
/// <param name="timeStamp">The timestamp of when the cheep was sent</param>
/// <param name="id">The unique ID of the cheep. Note that this is the autoincrement primary key in the database, and thus EF-core takes automatic care of assigning it </param>
public class CheepDTO(string author, string message, DateTime timeStamp, int id)
{
    public int Id { get; set; } = id;
    // Identifies the author, whose name is the primary key 
    public string AuthorName { get; set; } = author;
    public string MessageContent { get; set; } = message;
    // One could debate whether the CheepDTO class should be responsible for timestamp conversion logic, but doing it in this manner allows us to easily convert Cheeps directly to CheepDTOs, plus wrt KISS it's much simpler than creating a seperate class with the sole function of converting DateTime objects to strings
    public string TimeStampStr { get; set; } = timeStamp.ToString("MM/dd/yy H:mm:ss", CultureInfo.InvariantCulture);

    public CheepDTO(Cheep cheep) : this(
        cheep.Author?.UserName ?? throw new ArgumentNullException(nameof(cheep.Author), "Author cannot be null."),
        cheep.Text ?? throw new ArgumentNullException(nameof(cheep.Text), "Cheep text cannot be null."),
        cheep.TimeStamp,
        cheep.Id)
    { }
}