using System.ComponentModel.DataAnnotations;
using DataAccess.Models;
using Newtonsoft.Json;

namespace Api.DTO;

public record CreateEventRequestDto
{
    public  DateTimeOffset? StartUtc { get; set; }
    
    public DateTimeOffset? EndUtc { get; set; }

    //blank title if the user doesn't manually enter one
    public string Title { get; set; } = "";

    public bool IsAllDay { get; set; } = false;
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? TimeZoneId { get; set; }
    
    public EventColor? Color { get; set; }
    
    
}