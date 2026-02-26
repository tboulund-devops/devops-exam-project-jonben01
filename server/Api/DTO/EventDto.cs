using System.ComponentModel.DataAnnotations;
using DataAccess.Models;

namespace Api.DTO;

public record EventDto
{
    //potentially expand with event creator in case of adding multi user calendars.
    
    public Guid Id { get; set; }
    public DateTimeOffset? StartUtc { get; set; }
    public DateTimeOffset? EndUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string Title { get; set; } = "";
    
    public EventColor Color { get; set; }
    
    public bool IsAllDay { get; set; } = false;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    
    public string? TimeZoneId { get; set; }
}