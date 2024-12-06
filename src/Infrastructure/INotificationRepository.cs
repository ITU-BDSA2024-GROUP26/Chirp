using Core; 

namespace Infrastructure;

public interface INotificationRepository 
{
    public Task<ICollection<Notification>> GetNotifications(string authorName, bool getOld);// get the pending notifications of the author in question
    public Task CreateNotification(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Cheep> cheepTracker);
}