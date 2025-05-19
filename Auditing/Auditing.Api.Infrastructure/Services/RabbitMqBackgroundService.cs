using Microsoft.Extensions.Hosting;

namespace Auditing.Api.Infrastructure.Services
{
    public class RabbitMqBackgroundService : BackgroundService
    {
        private readonly RabbitMqLogConsumer _consumer;

        public RabbitMqBackgroundService(RabbitMqLogConsumer consumer)
        {
            _consumer = consumer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _consumer.StartListeningAsync();
        }
    }
}
