using Auditing.Api.Domain.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Auditing.Api.Infrastructure.Services
{
    public class RabbitMqLogConsumer
    {
        private readonly string _hostname = "localhost"; // RabbitMQ hostname
        private readonly string _queueName = "logs";

        public async void StartListening()
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var log = JsonSerializer.Deserialize<Log>(message);

                // Process the log (e.g., save to database)
                Console.WriteLine($"Received log: {log?.Message}");
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);

            Console.WriteLine("Listening for logs...");
            Console.ReadLine();
        }
    }
}