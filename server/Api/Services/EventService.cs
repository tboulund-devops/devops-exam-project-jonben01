using System.ComponentModel.DataAnnotations;
using Api.DTO;
using DataAccess;
using DataAccess.Models;

namespace Api.Services;

public class EventService : IEventService
{

    private readonly MyDbContext _context;
    
    public EventService(MyDbContext context)
    {
        _context = context;
    }
    
    public async Task<EventDto> CreateEvent(
        Guid userId,
        CreateEventRequestDto createEventRequestDto, 
        CancellationToken ct)
    {
        
        //TODO move validation to helper methods -- will be nice for when event types gets added, to avoid bloat.
        
        if (createEventRequestDto.IsAllDay)
        {
            if (createEventRequestDto.StartDate is null || createEventRequestDto.EndDate is null)
            {
                throw new ValidationException("Start date and end date are required for all day events");
            }
            
            if (createEventRequestDto.StartDate > createEventRequestDto.EndDate)
            {
                throw new ValidationException("Start date cannot be after end date.");
            }
        }
        else
        {
            //maybe only keep start utc, and automatically set end utc to end of day
            if (createEventRequestDto.StartUtc is null || createEventRequestDto.EndUtc is null)
                throw new ValidationException("Start time cannot be null.");
            
            if (createEventRequestDto.StartUtc >= createEventRequestDto.EndUtc)
                throw new ValidationException("Start time cannot be after end time.");
            
            if (!IsValidTimezone(createEventRequestDto.TimeZoneId))
                throw new ValidationException("Time zone is not valid.");
            
        }
        
        
        var now =  DateTimeOffset.UtcNow;
        var evt = new Event
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Title = createEventRequestDto.Title,
            IsAllDay = createEventRequestDto.IsAllDay,
            StartUtc = createEventRequestDto.StartUtc,
            EndUtc =  createEventRequestDto.EndUtc,
            StartDate = createEventRequestDto.StartDate,
            EndDate = createEventRequestDto.EndDate,
            TimeZoneId = createEventRequestDto.TimeZoneId,
            CreatedAt = now,
            UpdatedAt =  now,
            
        };
        
        _context.Events.Add(evt);
        await _context.SaveChangesAsync(ct);
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
    
    private static bool IsValidTimezone(string? timezoneId) =>
        timezoneId is not null && TimeZoneInfo.TryFindSystemTimeZoneById(timezoneId, out _);
    
}