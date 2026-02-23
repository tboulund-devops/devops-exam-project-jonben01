using DataAccess.Models;

namespace Api.DTO.util;

public static class EntityToDtoMapper
{
    public static EventDto ToDto(this Event evt)
    {
        return new EventDto
        {
            Id = evt.Id,
            Title = evt.Title,
            IsAllDay = evt.IsAllDay,
            StartUtc = evt.StartUtc,
            EndUtc = evt.EndUtc,
            StartDate = evt.StartDate,
            EndDate = evt.EndDate,
            TimeZoneId = evt.TimeZoneId,
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,  
        };
    }
    
    
}