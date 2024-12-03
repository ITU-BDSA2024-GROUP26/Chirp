using System.ComponentModel.DataAnnotations;

namespace Core;


public class Notification(Cheep cheep, Author authorToNotify,  bool tagNotification)
{
    [Required]
    public required Cheep cheep {get; set;} = cheep; // the cheep the notification is in reference to 
    [Required]
    public required Author authorToNotify { get; set;} = authorToNotify;
    [Required]
    public required bool tagNotification { get; set; } = tagNotification; // are we notifying due to the author being tagged or following 
}