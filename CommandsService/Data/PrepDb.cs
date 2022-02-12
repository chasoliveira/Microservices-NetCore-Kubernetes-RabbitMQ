using CommandeService.SyncDataServices.Grpc;
using CommandsService.Models;

namespace CommandsService.Data;

public static class PrepDb
{
  public static void PrepPopulation(IApplicationBuilder app)
  {
    using var serviceScoped = app.ApplicationServices.CreateScope();
    var grpcClient = serviceScoped.ServiceProvider.GetService<IPlatformDataClient>();
    var platforms = grpcClient.ReturnAllPlatforms();
    SeedData(serviceScoped.ServiceProvider.GetService<ICommandRepo>(), platforms);
  }

  private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
  {
    Console.WriteLine("---> Seeding new platforms...");
    foreach (var plat in platforms)
    {
      if (!repo.ExternalPlatformExists(plat.ExternalId))
        repo.CreatePlatform(plat);
      repo.SaveChanges();
    }
  }
}