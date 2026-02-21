using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models;

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // If needed, add the actual user, to avoid unnecessary db calls
    [Required]
    public Guid OwnerId { get; set; }

    [Required, MaxLength(200)] 
    public string Title { get; set; } = "";
    
    [Required]
    public DateTimeOffset StartUtc { get; set; }
    
    [Required]
    public DateTimeOffset EndUtc { get; set; }
    
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /*
     * Could add
     * 
     * Owner entity,
     * Reminder time,
     * Color-coding,
     * Notes/Description
     */
}