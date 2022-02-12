using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using PlatformService;
using static PlatformService.GrpcPlatform;

namespace CommandeService.SyncDataServices.Grpc;

public interface IPlatformDataClient
{
  IEnumerable<Platform> ReturnAllPlatforms();
}

public class PlatformDataClient : IPlatformDataClient
{
  private readonly IConfiguration _configuration;
  private readonly IMapper _mapper;

  public PlatformDataClient(IConfiguration configuration, IMapper mapper)
  {
    _configuration = configuration;
    _mapper = mapper;
  }
  public IEnumerable<Platform> ReturnAllPlatforms()
  {
    var address = _configuration["GrpcPlatform"];
    Console.WriteLine($"---> Calling gRPC Platform Services {address}");

    var channel = GrpcChannel.ForAddress(address);
    var client = new GrpcPlatformClient(channel);
    var request = new GetAllRequest();
    try
    {
      var repy = client.GetAllPlatforms(request);
      return _mapper.Map<IEnumerable<Platform>>(repy.Platform);
    }
    catch (System.Exception ex)
    {
      Console.WriteLine($"---> Could not call gRPC Platform Services: { ex.Message}");
      return null;
    }
  }
}