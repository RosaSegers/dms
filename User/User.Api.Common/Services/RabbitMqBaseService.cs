using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace User.Api.Common.Services
{
    public abstract class RabbitMqService : IAsyncDisposable
    {
        protected readonly IConnection _connection;
        protected readonly IChannel _channel;

        protected RabbitMqService(string hostname)
        {
            var factory = new ConnectionFactory
            {
                HostName = hostname
            };

            // Async connect and create channel synchronously here for simplicity  
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
        }

        protected async Task DeclareQueueAsync(string queueName)
        {
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        protected async Task PublishAsync(string queueName, SagaMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            // Specify the type argument explicitly for BasicPublishAsync

            var props = new BasicProperties();
            props.Persistent = true;

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: true,
                basicProperties: props,
                body: body);
        }


        protected async Task StartConsumer(string queueName, Func<SagaMessage, Task> onMessageAsync)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<SagaMessage>(json);

                if (message != null)
                {
                    await onMessageAsync(message);
                }

                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }
    }

    public class SagaMessage
    {
        public string SagaId { get; set; }
        public string Type { get; set; }
        public JsonElement Payload { get; set; }
    }
}
