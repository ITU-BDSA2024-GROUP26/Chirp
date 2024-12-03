using System.ComponentModel.DataAnnotations;

namespace Core;


public class Notification
{
    [Required]
    public required int cheepID {get; set; } // the cheep the notification is in reference to 
    [Required]
    public required string authorID { get; set;}
    [Required]
    public required bool tagNotification { get; set; }// are we notifying due to the author being tagged or following 

    public Cheep cheep {get; set;}
    public Author  authorToNotify { get; set; }
}