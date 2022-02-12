using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlatformsController : ControllerBase
{
  private readonly IPlatformRepo _repository;
  private readonly IMapper _mapper;
  private readonly ICommandDataClient _commandDataClient;
  private readonly IMessageBusClient _messageBusClient;

  public PlatformsController(
    IPlatformRepo repository,
    IMapper mapper,
    ICommandDataClient commandDataClient,
    IMessageBusClient messageBusClient)
  {
    _repository = repository;
    _mapper = mapper;
    _commandDataClient = commandDataClient;
    _messageBusClient = messageBusClient;
  }

  [HttpGet]
  public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
  {
    Console.WriteLine("---> Getting Platforms...");
    var platforms = _repository.GetAllPlatforms();
    return Ok(_mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
  }

  [HttpGet("{id}", Name = "GetPlatformById")]
  public ActionResult<PlatformReadDto> GetPlatformById(int id)
  {
    Console.WriteLine($"---> Getting Platform for {id}...");
    var platform = _repository.GetPlatformById(id);
    if (platform is null) return NotFound();

    return Ok(_mapper.Map<PlatformReadDto>(platform));
  }

  [HttpPost]
  public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto platform)
  {
    Console.WriteLine($"---> Creating a new Platform...");
    var platformModel = _mapper.Map<Platform>(platform);
    _repository.CreatePlatform(platformModel);
    _repository.SaveChanges();

    var platformReadDto = _mapper.Map<PlatformReadDto>(platformModel);

    //Send Sync Message
    try
    {
      await _commandDataClient.SendPlatformToCommand(platformReadDto);
    }
    catch (System.Exception ex)
    {
      Console.WriteLine($"---> Could not send synchronously: {ex.Message}");
    }

    //Send Async Message
    try
    {
      var platformPublishedDto = _mapper.Map<PlatformPublishedDto>(platformReadDto);
      platformPublishedDto.Event = "Platform_Published";
      _messageBusClient.PublishNewPlatform(platformPublishedDto);

    }
    catch (System.Exception ex)
    {
      Console.WriteLine($"---> Could not send asynchronously: {ex.Message}");
    }

    return CreatedAtRoute(nameof(GetPlatformById), new { Id = platformReadDto.Id }, platformReadDto);
  }
}