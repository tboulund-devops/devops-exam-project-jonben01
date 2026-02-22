using Api.DTO;
using Api.Services;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Testing;

public class CreateEventTest : IDisposable
{
    private readonly MyDbContext _dbContext;
    private readonly CreateEventService _createEventService;
    
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
        _createEventService = new CreateEventService(_dbContext);
    }
    
    [Fact]
    public async Task CreateEvent_TimedEvent_PersistsAndReturnsCorrectDto()
    {
        //Arrange
        var start = new DateTimeOffset(2025, 3, 1, 9, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2025, 3, 1, 10, 0, 0, TimeSpan.Zero);
        var before = DateTimeOffset.UtcNow;

        //Act
        var result = await _createEventService.CreateEvent(UserId, new CreateEventRequestDto
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
        var result = await _createEventService.CreateEvent(UserId, new CreateEventRequestDto
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
    
    //Test is to kill >= to > mutants
    [Fact]
    public async Task CreateEvent_AllDay_StartEqualsEnd_Succeeds()
    {
        //Arrange
        var date = new DateOnly(2025, 3, 1);

        //Act
        var result = await _createEventService.CreateEvent(UserId, new CreateEventRequestDto
        {
            IsAllDay  = true,
            StartDate = date,
            EndDate   = date,
        }, CancellationToken.None);

        //Assert
        Assert.Equal(date, result.StartDate);
        Assert.Equal(date, result.EndDate);
    }
    
    
}