using Api.Services;
using DataAccess;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Testing;

public class GetEventsTest : IDisposable
{
    private readonly MyDbContext _dbContext;
    private readonly EventService _eventService;
    private readonly Guid _userId = Guid.NewGuid();
    
    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    public GetEventsTest()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MyDbContext(options);
        _eventService = new EventService(_dbContext);
    }
    
    private Event TimedEvent(DateTimeOffset start, DateTimeOffset end) => new()
    {
        Id = Guid.NewGuid(),
        OwnerId = _userId,
        Title = "Timed Event",
        IsAllDay = false,
        StartUtc = start,
        EndUtc = end,
        CreatedAt = DateTimeOffset.Now,
        UpdatedAt = DateTimeOffset.Now,
    };

    private Event AllDayEvent(DateOnly start, DateOnly end) => new()
    {
        Id = Guid.NewGuid(),
        OwnerId = _userId,
        Title = "All Day Event",
        IsAllDay = true,
        StartDate = start,
        EndDate = end,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
    };

    private async Task SaveEventsAsync(params Event[] events)
    {
        _dbContext.Events.AddRange(events);
        await _dbContext.SaveChangesAsync();
    }
    
    
    [Fact]
    public async Task TimedEvent_WithinMonth_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 2, 10, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 10, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task TimedEvent_CompletelyBeforeMonth_IsExcluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 10, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task TimedEvent_CompletelyAfterMonth_IsExcluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 3, 5, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 6, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task AllDayEvent_WithinMonth_IsIncluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 12)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task AllDayEvent_CompletelyBeforeMonth_IsExcluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 20)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task AllDayEvent_CompletelyAfterMonth_IsExcluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 10)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task Event_BelongingToDifferentUser_IsExcluded()
    {
        var otherEvent = TimedEvent(
            new DateTimeOffset(2026, 2, 10, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 10, 10, 0, 0, TimeSpan.Zero));
        otherEvent.OwnerId = Guid.NewGuid();
        await SaveEventsAsync(otherEvent);

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task NoEvents_ReturnsEmptyList()
    {
        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    
    [Fact]
    public async Task MultipleEvents_AllReturned()
    {
        await SaveEventsAsync(
            TimedEvent(
                new DateTimeOffset(2026, 2, 5, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 2, 5, 10, 0, 0, TimeSpan.Zero)),
            AllDayEvent(new DateOnly(2026, 2, 10), new DateOnly(2026, 2, 11)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
    
    [Fact]
    public async Task ReturnedDto_HasCorrectFields()
    {
        var evt = TimedEvent(
            new DateTimeOffset(2026, 2, 10, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 10, 10, 0, 0, TimeSpan.Zero));
        evt.Title = "My Event";
        await SaveEventsAsync(evt);

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        var dto = Assert.Single(result);
        Assert.Equal(evt.Id, dto.Id);
        Assert.Equal("My Event", dto.Title);
        Assert.False(dto.IsAllDay);
        Assert.Equal(evt.StartUtc, dto.StartUtc);
        Assert.Equal(evt.EndUtc, dto.EndUtc);
    }
    
    //Should show events from last month, if visible on grid (starts on monday)
    [Fact]
    public async Task TimedEvent_StartsOnGridStartDay_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 1, 26, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 26, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task TimedEvent_EndsOneDayBeforeGridStart_IsExcluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 1, 20, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 25, 23, 59, 59, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task TimedEvent_EndsOnGridEndDay_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 1, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task TimedEvent_StartsOneDayAfterGridEnd_IsExcluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 3, 2, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task TimedEvent_StartsBeforeGridEndsWithinGrid_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 1, 20, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 3, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    
    [Fact]
    public async Task TimedEvent_StartsWithinGridEndsAfterGrid_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2026, 2, 26, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 3, 10, 10, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task TimedEvent_SpansEntireGrid_IsIncluded()
    {
        await SaveEventsAsync(TimedEvent(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task AllDayEvent_StartsOnGridStartDate_IsIncluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 1, 26), new DateOnly(2026, 1, 26)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task AllDayEvent_EndsOneDayBeforeGridStart_IsExcluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 1, 20), new DateOnly(2026, 1, 25)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task AllDayEvent_EndsOnGridEndDate_IsIncluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 1)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
    
    [Fact]
    public async Task AllDayEvent_StartsOneDayAfterGridEnd_IsExcluded()
    {
        await SaveEventsAsync(AllDayEvent(new DateOnly(2026, 3, 2), new DateOnly(2026, 3, 5)));

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Empty(result);
    }
    
    [Fact]
    public async Task EventsFromMultipleUsers_OnlyRequestingUsersAreReturned()
    {
        var otherEvent = TimedEvent(
            new DateTimeOffset(2026, 2, 15, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 2, 15, 10, 0, 0, TimeSpan.Zero));
        otherEvent.OwnerId = Guid.NewGuid();

        await SaveEventsAsync(
            TimedEvent(
                new DateTimeOffset(2026, 2, 10, 9, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2026, 2, 10, 10, 0, 0, TimeSpan.Zero)),
            otherEvent);

        var result = await _eventService.GetEventsForMonth(_userId, 2026, 2, CancellationToken.None);

        Assert.Single(result);
    }
}