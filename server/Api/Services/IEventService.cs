using Api.DTO;

namespace Api.Services;

public interface IEventService
{
    Task<EventDto> CreateEvent(Guid userId, CreateEventRequestDto createEventRequestDto, CancellationToken ct);
    
    Task<List<EventDto>> GetEventsForMonth(Guid userId, int year, int month, CancellationToken ct);
    
    //Todo update and delete
}  