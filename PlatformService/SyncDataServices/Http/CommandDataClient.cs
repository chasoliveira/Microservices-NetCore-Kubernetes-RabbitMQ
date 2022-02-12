using System.Text;
using System.Text.Json;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http;

public class CommandDataClient : ICommandDataClient
{
  private readonly HttpClient _httpClient;

  public CommandDataClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task SendPlatformToCommand(PlatformReadDto platform)
  {
    var httpContent = new StringContent(JsonSerializer.Serialize(platform), Encoding.UTF8, "application/json");
    var response = await _httpClient.PostAsync("/api/c/platforms", httpContent);
    if (response.IsSuccessStatusCode)
      Console.WriteLine("---> Sync POST to CommandService was OK!");
    else
      Console.WriteLine("---> Sync POST to CommandService was NOT OK!");
  }
}