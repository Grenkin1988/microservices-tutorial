using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CommandsService.EventProcessing;

public class EventProcessor : IEventProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMapper _mapper;
    private readonly ILogger<EventProcessor> _logger;

    public EventProcessor(IServiceScopeFactory scopeFactory,
        IMapper mapper,
        ILogger<EventProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _mapper = mapper;
        _logger = logger;
    }

    public void ProcessEvent(string message)
    {
        var eventType = DetermineEvent(message);

        switch (eventType)
        {
            case EventType.PlatformPublished:
                AddPlatform(message);
                break;
            default:
                break;
        }
    }

    private EventType DetermineEvent(string notificationMessage)
    {
        _logger.LogDebug("Determining Event");

        var @event = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);
        var eventType = MapToEventType(@event.Event);
        _logger.LogDebug("{EventType} Event Detected", eventType);
        return eventType;
    }

    private void AddPlatform(string platformMessage)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

        var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformMessage);

        try
        {
            var platform = _mapper.Map<Platform>(platformPublishedDto);
            if (!repo.ExternalPlatformExist(platform.ExternalId))
            {
                repo.CreatePlatform(platform);
                repo.SaveChanges();
                _logger.LogInformation("Platform {ExternalId} added", platform.ExternalId);
            }
            else
            {
                _logger.LogInformation("Platform {ExternalId} already exist", platform.ExternalId);
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Could not add Platform to DB");
        }
    }

    private static EventType MapToEventType(string eventType) => eventType switch
    {
        "Platform_Published" => EventType.PlatformPublished,
        _ => EventType.Undetermined
    };
}

internal enum EventType
{
    PlatformPublished,
    Undetermined
}
