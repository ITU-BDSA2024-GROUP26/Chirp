using System.ComponentModel.DataAnnotations;

namespace Core;

/// <summary>
/// A notification is addressed to a certain author, from a certain author, referencing some cheep. 
/// Notifications from A to B can either be because B follows A, or because A tagged B. Tags take precedence over following. 
/// </summary>
public class Notification
{
    // Cheep the notification is in reference to 
    // Note that doing it in this manner, instead of a reference to a Cheep object, allowed us to ensure that EF-core makes the primary key be a composite of (CheepID, AuthorID).
    // Which is what we want for our case, since a certain author can be notified about several cheeps, and a particular cheep can notify several authors. 
    // But an author should only recieve one notification for every cheep. 
    [Required]
    public required int cheepID {get; set; } 
    // The ID of the author who recieves the notification. Note that we have a composite primary key of (cheepID, authorID). 
    [Required]
    public required string authorID { get; set;}
    [Required]
    public required bool tagNotification { get; set; }// are we notifying due to the author being tagged or following 
    // A simple flag for whether this is the first time we query for this notification or not. 
    // This allows the frontend to very easily query periodically only for *new* notifications.  
    [Required] 
    public required bool isNew {get; set;}

    // Bit of a double reference when we have the cheepID, but like we explained we need seperate cheepID to create the composite primary key.
    // While also needing an actual cheep object to ensure a proper foreign key relation to the cheeps table
    public Cheep cheep {get; set;}
    // And same logic here, need a seperate authorID to create the composite primary key. 
    public Author  authorToNotify { get; set; }
}