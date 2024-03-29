﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using System.Collections.Generic;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
    private readonly IPlatformRepo _repository;
    private readonly IMapper _mapper;
    private readonly IMessageBusClient _messageBusClient;
    private readonly ILogger<PlatformsController> _logger;

    public PlatformsController(
        IPlatformRepo repository,
        IMapper mapper,
        IMessageBusClient messageBusClient,
        ILogger<PlatformsController> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _messageBusClient = messageBusClient;
        _logger = logger;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
    {
        var platformItems = _repository.GetAllPlatforms();

        return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platformItems));
    }

    [HttpGet("{id}", Name = nameof(GetPlatformById))]
    public ActionResult<PlatformReadDto> GetPlatformById(int id)
    {
        var platformItem = _repository.GetPlatformById(id);

        return platformItem is not null ?
            Ok(_mapper.Map<PlatformReadDto>(platformItem))
            : NotFound();
    }

    [HttpPost]
    public ActionResult<PlatformReadDto> CreatePlatform(PlatformCreateDto platformCreateDto)
    {
        var platform = _mapper.Map<Platform>(platformCreateDto);
        _repository.CreatePlatform(platform);
        _repository.SaveChanges();

        var platformReadDto = _mapper.Map<PlatformReadDto>(platform);
        _logger.LogInformation("Platform created. {PlatformId}", platformReadDto.Id);

        try
        {
            var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
            platformPublishedDto = platformPublishedDto with { Event = "Platform_Published" };
            _messageBusClient.PublishNewPlatform(platformPublishedDto);
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Could not send message");
        }

        return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
    }
}
