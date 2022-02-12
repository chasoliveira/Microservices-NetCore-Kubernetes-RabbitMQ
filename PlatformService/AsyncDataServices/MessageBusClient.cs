using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices;
public interface IMessageBusClient
{
  void PublishNewPlatform(Dtos.PlatformPublishedDto platform);
}
public class MessageBusClient : IMessageBusClient
{
  private readonly IConfiguration _configuration;
  private readonly IConnection _connection;
  private readonly IModel _channel;

  public MessageBusClient(IConfiguration configuration)
  {
    _configuration = configuration;

    var factory = new ConnectionFactory()
    {
      HostName = _configuration["RabbitMQHost"],
      Port = int.Parse(_configuration["RabbitMQPort"])
    };

    try
    {
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
      _channel.ExchangeDeclare("trigger", ExchangeType.Fanout);

      _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

      Console.WriteLine($"---> Connnect to MessageBus.");
    }
    catch (System.Exception ex)
    {
      Console.WriteLine($"---> Could not connnect to the Message Bus: {ex.Message}");
    }
  }

  public void PublishNewPlatform(PlatformPublishedDto platform)
  {
    var message = JsonSerializer.Serialize(platform);
    if (_connection.IsOpen)
    {
      Console.WriteLine($"---> RabbitMQ Connection is Open, sending message...");
      SendMessage(message);
    }
    else
      Console.WriteLine($"---> RabbitMQ Connection is closed, not sending message...");
  }

  private void SendMessage(string message)
  {
    var body = Encoding.UTF8.GetBytes(message);
    _channel.BasicPublish("trigger", "", true, null, body);

    Console.WriteLine($"---> We have sent{message}");
  }

  private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
  {
    Console.WriteLine($"---> RabbitMQ Connnect Shutdown: {e}");
  }

  public void Dispose()
  {
    Console.WriteLine($"---> MessageBus Disposed");
    if (_channel.IsOpen)
    {
      _channel.Close();
      _connection.Close();
    }

  }
}