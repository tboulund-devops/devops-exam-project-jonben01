using Api.DTO;

namespace Api.Services;

public interface IEventService
{
    Task<EventDto> CreateEvent(Guid userId, CreateEventRequestDto createEventRequestDto, CancellationToken ct);
}