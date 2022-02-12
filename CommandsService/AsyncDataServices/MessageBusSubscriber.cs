using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataSerices;

public class MessageBusSubscriber : BackgroundService
{
  private readonly IConfiguration _configuration;
  private readonly IEventProcessor _evenProcessor;
  private IConnection _connection;
  private IModel _channel;
  private string _queueName;

  public MessageBusSubscriber(IConfiguration configuration, IEventProcessor evenProcessor)
  {
    _configuration = configuration;
    _evenProcessor = evenProcessor;
    InitializeRabbitMQ();
  }

  private void InitializeRabbitMQ()
  {
    var factory = new ConnectionFactory()
    {
      HostName = _configuration["RabbitMQHost"],
      Port = int.Parse(_configuration["RabbitMQPort"]),
    };

    _connection = factory.CreateConnection();
    _channel = _connection.CreateModel();
    _channel.ExchangeDeclare("trigger", ExchangeType.Fanout);
    _queueName = _channel.QueueDeclare().QueueName;
    _channel.QueueBind(_queueName, "trigger", "");
    Console.WriteLine("---> Listening on the Message Bus...");

    _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
  }

  private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
  {
    Console.WriteLine("---> Connection Shutdown");
  }

  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    stoppingToken.ThrowIfCancellationRequested();
    var consumer = new EventingBasicConsumer(_channel);
    consumer.Received += (ModuleHandle, ea) =>{
      Console.WriteLine("---> Event Received!");
      var body = ea.Body;
      var notificationMessage = Encoding.UTF8.GetString(body.ToArray());
      _evenProcessor.ProcessEvent(notificationMessage);
    };

    _channel.BasicConsume(_queueName, autoAck: true, consumer: consumer);

    return Task.CompletedTask;
  }

  public override void Dispose()
  {
    if (_channel.IsOpen)
    {
      _channel.Close();
      _connection.Close();
    }
    base.Dispose();
  }
}