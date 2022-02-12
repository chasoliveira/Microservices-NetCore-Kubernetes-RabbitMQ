using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers;

[ApiController]
[Route("api/c/[controller]")]
public class PlatformsController : ControllerBase
{
  private readonly ICommandRepo _repository;
  private readonly IMapper _mapper;

  public PlatformsController(ICommandRepo repository, IMapper mapper)
  {
    _repository = repository;
    _mapper = mapper;
  }

  [HttpGet]
  public ActionResult<IEnumerable<PlatformReadDto>> GetAllPlatforms()
  {
    Console.WriteLine("---> Getting Platforms from CommandsService...");
    var platforms = _repository.GetAllPlatforms();
    var platformsDto = _mapper.Map<IEnumerable<PlatformReadDto>>(platforms);

    return Ok(platformsDto);
  }

  [HttpPost]
  public ActionResult TestInboundConnection()
  {
    Console.WriteLine("---> Inbound POST # Command Service");
    return Ok("Inbound test of from Platforms Controller");
  }
}