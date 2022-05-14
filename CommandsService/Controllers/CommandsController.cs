using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepo _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<CommandsController> _logger;

        public CommandsController(
            ICommandRepo repository,
            IMapper mapper,
            ILogger<CommandsController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsForPlatform(int platformId)
        {
            _logger.LogInformation("Getting Commnads for Platform:{PlatformId}", platformId);

            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commandItems = _repository.GetCommandsFormPlatform(platformId);

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{commandId}", Name = nameof(GetCommandForPlatform))]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            _logger.LogInformation("Getting Commnad[{CommandId}] for Platform:{PlatformId}", commandId, platformId);

            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commandItem = _repository.GetCommand(platformId, commandId);

            if (commandItem is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandCreateDto)
        {
            _logger.LogInformation("Creating Command for Platform:{PlatformId}", platformId);

            if (!_repository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = _mapper.Map<Command>(commandCreateDto);
            _repository.CreateCommand(platformId, command);
            _repository.SaveChanges();

            var commandReadDto = _mapper.Map<CommandReadDto>(command);

            var param = new
            {
                platformId = commandReadDto.PlatformId,
                commandId = commandReadDto.Id
            };
            return CreatedAtRoute(nameof(GetCommandForPlatform), param, commandReadDto);
        }
    }
}
