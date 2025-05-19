using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Organization.Api.Infrastructure.Services
{
    public class RabbitMqLogProducer : IDisposable
    {
        private readonly string _hostname = "localhost"; // RabbitMQ hostname
        private readonly string _queueName = "logs";
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        public RabbitMqLogProducer(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = _hostname,
                UserName = configuration.GetRequiredSection("RabbitMQ:Credentials:Username").Value ?? "",
                Password = configuration.GetRequiredSection("RabbitMQ:Credentials:Password").Value ?? ""
            };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
            _channel.QueueDeclareAsync(_queueName, durable: false, exclusive: false, autoDelete: false, arguments: null).Wait();
        }

        public async void PublishLog(object log)
        {
            var logMessage = JsonSerializer.Serialize(log);
            var body = Encoding.UTF8.GetBytes(logMessage);

            await _channel.BasicPublishAsync(exchange: "", routingKey: _queueName, body: body);
        }

        public async void Dispose()
        {
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
        }
    }
}
