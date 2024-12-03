namespace Core;

public record NotificationDTO(string cheepContent, string authorName, bool tagNotification)
{
    public override string ToString()
    {
        if (tagNotification) 
        {
            return authorName + " tagged you in a cheep!"; 
        } else {
            return authorName + " chirped!"; 
        }
    }
}