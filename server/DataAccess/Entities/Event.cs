using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class Event
{
    
    //TODO add a type + enum for holidays or recurring (maybe add something like a recurrence rule for recurring though)
    // like FREQ=WEEKLY or FREQ=MONTHLY and day etc..
    
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // If needed, add the actual user, to avoid unnecessary db calls
    public Guid OwnerId { get; set; }
    
    public string Title { get; set; } = "";


    //All day event stuff
    public bool IsAllDay { get; set; } = false;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    
    public DateTimeOffset? StartUtc { get; set; }
    
    public DateTimeOffset? EndUtc { get; set; }
    
    public string? TimeZoneId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    public EventColor Color { get; set; } 
    
    /*
     * Could add
     * 
     * Owner entity,
     * Reminder time,
     * Notes/Description
     */
}