using System.ComponentModel.DataAnnotations;

namespace Core;


public class Notification(Cheep cheep, Author authorToNotify,  bool tagNotification)
{
    [Required]
    public Cheep cheep {get; set;} = cheep; // the cheep the notification is in reference to 
    [Required]
    public Author authorToNotify { get; set;} = authorToNotify;
    [Required]
    public bool tagNotification { get; } = tagNotification; // are we notifying due to the author being tagged or following 
}