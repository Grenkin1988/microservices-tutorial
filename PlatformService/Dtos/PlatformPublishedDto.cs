namespace PlatformService.Dtos;

public record PlatformPublishedDto(
    int Id,
    string Name,
    string Event = "");
