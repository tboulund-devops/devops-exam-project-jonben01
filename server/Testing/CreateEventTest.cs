using System.ComponentModel.DataAnnotations;
using Api.DTO;
using Api.Services;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Testing;

public class CreateEventTest : IDisposable
{
    private readonly MyDbContext _dbContext;
    private readonly EventService _eventService;
    
    private const string ValidTimezone = "Europe/Copenhagen";
    private static readonly Guid UserId = Guid.NewGuid();
    
    public void Dispose()
    {
        _dbContext.Dispose();
    }

    public CreateEventTest()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new MyDbContext(options);
        _eventService = new EventService(_dbContext);
    }
    
    [Fact]
    public async Task CreateEvent_TimedEvent_PersistsAndReturnsCorrectDto()
    {
        //Arrange
        var start = new DateTimeOffset(2025, 3, 1, 9, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero);
        var before = DateTimeOffset.UtcNow;

        //Act
        var result = await _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay = false,
            StartUtc = start,
            EndUtc = end,
            TimeZoneId = ValidTimezone,
            Title = "Meeting"
        }, CancellationToken.None);
        
        var saved = await _dbContext.Events.FindAsync(result.Id);
        
        //Assert
        Assert.Equal("Meeting", result.Title);
        Assert.Equal(start, result.StartUtc);
        Assert.Equal(end, result.EndUtc);
        Assert.Equal(ValidTimezone, result.TimeZoneId);
        Assert.False(result.IsAllDay);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt >= before);
        Assert.Equal(result.CreatedAt, result.UpdatedAt);
        Assert.NotNull(saved);
        Assert.Equal(UserId, saved.OwnerId);
    }
    
    [Fact]
    public async Task CreateEvent_AllDayEvent_PersistsAndReturnsCorrectDto()
    {
        //Arrange
        var start = new DateOnly(2025, 3, 1);
        var end = new DateOnly(2025, 3, 3);

        //Act
        var result = await _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay  = true,
            StartDate = start,
            EndDate   = end,
            Title     = "Holiday"
        }, CancellationToken.None);
        
        var saved = await _dbContext.Events.FindAsync(result.Id);
        
        //Assert
        Assert.True(result.IsAllDay);
        Assert.Equal(start, result.StartDate);
        Assert.Equal(end, result.EndDate);
        Assert.Null(result.StartUtc);
        Assert.Null(result.EndUtc);
        Assert.NotNull(saved);
        Assert.Equal(UserId, saved.OwnerId);
    }
    
    //The following Tests mostly focus on killing >= to > or similar mutants.
    
    [Fact]
    public async Task CreateEvent_AllDay_StartEqualsEnd_Succeeds()
    {
        //Arrange
        var date = new DateOnly(2025, 3, 1);

        //Act
        var result = await _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay  = true,
            StartDate = date,
            EndDate   = date,
        }, CancellationToken.None);

        //Assert
        Assert.Equal(date, result.StartDate);
        Assert.Equal(date, result.EndDate);
    }
    
    [Theory]
    [InlineData(true,  false)]
    [InlineData(false, true)]
    [InlineData(true,  true)]
    public async Task CreateEvent_TimedEvent_NullStartOrEnd_Throws(bool nullStart, bool nullEnd)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay = false,
            StartUtc = nullStart ? null : DateTimeOffset.UtcNow,
            EndUtc = nullEnd ? null : DateTimeOffset.UtcNow.AddHours(1),
            TimeZoneId = ValidTimezone,
        }, CancellationToken.None));
    }
    
    
    [Theory]
    [InlineData(3, 2)]  
    [InlineData(0, 0)]  
    public async Task CreateEvent_TimedEvent_StartNotBeforeEnd_Throws(int startHours, int endHours)
    {
        var now = DateTimeOffset.UtcNow;

        await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay = false,
            StartUtc = now.AddHours(startHours),
            EndUtc = now.AddHours(endHours),
            TimeZoneId = ValidTimezone,
        }, CancellationToken.None));
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("Not/ATimezone")]
    public async Task CreateEvent_TimedEvent_InvalidTimezone_Throws(string? timezoneId)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay = false,
            StartUtc = DateTimeOffset.UtcNow,
            EndUtc = DateTimeOffset.UtcNow.AddHours(1),
            TimeZoneId = timezoneId,
        }, CancellationToken.None));
    }
    
    [Theory]
    [InlineData(true,  false)]
    [InlineData(false, true)]
    [InlineData(true,  true)]
    public async Task CreateEvent_AllDay_NullStartOrEndDate_Throws(bool nullStart, bool nullEnd)
    {
        await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay  = true,
            StartDate = nullStart ? null : new DateOnly(2025, 3, 1),
            EndDate = nullEnd ? null : new DateOnly(2025, 3, 1),
        }, CancellationToken.None));
    }
    
    [Fact]
    public async Task CreateEvent_AllDay_StartAfterEnd_Throws()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _eventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay  = true,
            StartDate = new DateOnly(2025, 3, 5),
            EndDate   = new DateOnly(2025, 3, 1),
        }, CancellationToken.None));
    }
    
}