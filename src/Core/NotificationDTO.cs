namespace Core;

public record NotificationDTO(string CheepContent, string AuthorName, bool TagNotification)
{
    public override string ToString()
    {
        if (TagNotification) 
        {
            return AuthorName + " tagged you in a cheep!"; 
        } else {
            return AuthorName + " chirped!"; 
        }
    }
}