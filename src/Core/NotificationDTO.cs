namespace Core;

/// <summary>
/// A class that encapsulates the information that we need to display about a particular notification to the frontend, also reducing all types to primary ones. 
/// </summary>
/// <param name="CheepContent">The text content of the cheep</param>
/// <param name="AuthorName">The username(primary key) of the author who is to be notified</param>
/// <param name="TagNotification">Whether this notification originates from the author being tagged or not</param>
public record NotificationDTO(string CheepContent, string AuthorName, bool TagNotification)
{
    // This class needs no members beyond the ones declared in the constructor
}