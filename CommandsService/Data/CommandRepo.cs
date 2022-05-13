using CommandsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandsService.Data
{
    public class CommandRepo : ICommandRepo
    {
        private readonly AppDbContext _context;

        public CommandRepo(AppDbContext context)
        {
            _context = context;
        }

        public void CreateCommand(int platformId, Command command)
        {
            if(command is null)
            {
                throw new ArgumentException(nameof(command));
            }

            command.PlatformId = platformId;
            _context.Commands.Add(command);
        }
        
        public void CreatePlatform(Platform platform)
        {
            if(platform is null)
            {
                throw new ArgumentException(nameof(platform));
            }
            _context.Platforms.Add(platform);
        }

        public IEnumerable<Platform> GetAllPlatforms() => 
            _context.Platforms.ToList();
        
        public Command GetCommand(int platformId, int commandId) => 
            _context.Commands
                .FirstOrDefault(c => c.PlatformId == platformId && c.Id == commandId);
        
        public IEnumerable<Command> GetCommandsFormPlatform(int platformId) =>
            _context.Commands
                .Where(c => c.PlatformId == platformId)
                .OrderBy(c => c.Platform.Name);
        
        public bool PlatformExists(int platformId) => 
            _context.Platforms.Any(p => p.Id == platformId);
        
        public bool SaveChanges() => 
            _context.SaveChanges() >= 0;
    }
}
