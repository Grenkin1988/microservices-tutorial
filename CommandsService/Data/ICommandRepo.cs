using CommandsService.Models;
using System.Collections.Generic;

namespace CommandsService.Data;

public interface ICommandRepo
{
    bool SaveChanges();

    IEnumerable<Platform> GetAllPlatforms();
    void CreatePlatform(Platform platform);
    bool PlatformExists(int platformId);
    bool ExternalPlatformExist(int externalPlatformId);

    IEnumerable<Command> GetCommandsFormPlatform(int platformId);
    Command GetCommand(int platformId, int commandId);
    void CreateCommand(int platformId, Command command);
}
