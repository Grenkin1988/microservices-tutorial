using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlatformService.Dtos;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices;

public class MessageBusClient : IMessageBusClient, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageBusClient> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private const string ExchangeName = "trigger";

    public MessageBusClient(IConfiguration configuration,
        ILogger<MessageBusClient> logger)
    {
        _configuration = configuration;
        _logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQHost"],
            Port = int.Parse(_configuration["RabbitMQPort"])
        };
        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            _logger.LogInformation("Connected to MessageBus {RabbitMQHost}:{RabbitMQPort}",
                _configuration["RabbitMQHost"],
                _configuration["RabbitMQPort"]);
        }
        catch (System.Exception ex)
        {
            _logger.LogCritical(ex, "Could not connect to the Nessage Bus. {RabbitMQHost}:{RabbitMQPort}",
                _configuration["RabbitMQHost"],
                _configuration["RabbitMQPort"]);
            throw;
        }
    }

    public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
    {
        var message = JsonSerializer.Serialize(platformPublishedDto);

        if (_connection.IsOpen)
        {
            _logger.LogInformation("RabbitMQ Connection Open, sending message...");
            SendMessage(message);
        }
        else
        {
            _logger.LogWarning("RabbitMQ Connection Not Open");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(ExchangeName,
            routingKey: "",
            basicProperties: null,
            body: body);

        _logger.LogInformation("Message was sent. {Message}", message);
    }

    public void Dispose()
    {
        _logger.LogDebug("MessageBus Disposed");
        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }

    private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
    {
        _logger.LogInformation("RabbitMQ Connection Shutdown");
    }
}
