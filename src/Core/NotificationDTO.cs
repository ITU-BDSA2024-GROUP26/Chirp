namespace Core;

public record NotificationDTO(int cheepID, string authorName, bool tagNotification)
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

    public NotificationDTO(Notification n) : this(n.cheep.Id, n.cheep.Author!.UserName!, n.tagNotification) {}
}