using System.ComponentModel.DataAnnotations;
using Api.DTO;
using Api.DTO.util;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

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
        ValidateCreateEventRequest(createEventRequestDto);
        
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
        return evt.ToDto();
    }

    public async Task<List<EventDto>> GetEventsForMonth(Guid userId, int year, int month, CancellationToken ct)
    {
        var firstOfMonth = new DateOnly(year, month, 1);
        var lastOfMonth = new DateOnly(year, month, 1).AddMonths(1).AddDays(-1);
        
        //subtract so we start on monday. sunday = 0, so subtract 6. monday = 1, so subtract 0 days. tue = 2 = -1 and so on
        var gridStart = firstOfMonth.AddDays(-(((int)firstOfMonth.DayOfWeek + 6) % 7));
        
        //set end date to sunday
        var gridEnd = lastOfMonth.AddDays((7 - (int)lastOfMonth.DayOfWeek) % 7);
        
        var start = new DateTimeOffset(gridStart.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var end = new DateTimeOffset(gridEnd.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);

        var events = await _context.Events
            .Where(e => e.OwnerId == userId && (
                (!e.IsAllDay && e.StartUtc < end && e.EndUtc > start) ||
                (e.IsAllDay && e.StartDate <= gridEnd && e.EndDate >= gridStart)
            ))
            .ToListAsync(ct);

        return events.Select(e => e.ToDto()).ToList();
    }

    private static void ValidateCreateEventRequest(CreateEventRequestDto createEventRequestDto)
    {
        if (createEventRequestDto.IsAllDay)
            ValidateAllDayEvent(createEventRequestDto);
        else
            ValidateTimedEvent(createEventRequestDto);
    }

    private static void ValidateTimedEvent(CreateEventRequestDto createEventRequestDto)
    {
        if (createEventRequestDto.StartUtc is null || createEventRequestDto.EndUtc is null)
            throw new ValidationException("Start time cannot be null.");
            
        if (createEventRequestDto.StartUtc >= createEventRequestDto.EndUtc)
            throw new ValidationException("Start time cannot be after end time.");
            
        if (!IsValidTimezone(createEventRequestDto.TimeZoneId))
            throw new ValidationException("Time zone is not valid.");
    }

    private static void ValidateAllDayEvent(CreateEventRequestDto createEventRequestDto)
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

    private static bool IsValidTimezone(string? timezoneId) =>
        timezoneId is not null && TimeZoneInfo.TryFindSystemTimeZoneById(timezoneId, out _);
    
    
    
    
    
    
}