﻿using Auditing.Api.Common.Enums;
using Auditing.Api.Domain.Entities;
using Auditing.Api.Infrastructure.Persistance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Auditing.Api.Infrastructure.Services
{
    public class RabbitMqLogConsumer(IConfiguration config, IServiceScopeFactory scopeFactory)
    {
        private readonly string _hostname = config.GetSection("RabbitMQ:Host").Value ?? "";
        private readonly string _queueName = "logs";
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public async Task StartListeningAsync()
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var dto = JsonSerializer.Deserialize<Domain.DTO.Log>(message);

                    if (dto == null)
                        return;

                    var log = new Log
                    {
                        UserId = dto.UserId,
                        Message = dto.Message,
                        RequestName = dto.RequestName,
                        RequestId = dto.RequestId,
                        Metadata = dto.Metadata,
                        Severity = Enum.TryParse<LogSeverity>(dto.LogSeverity, out var severity)
                            ? severity
                            : LogSeverity.Information, // fallback if parsing fails
                        Type = Enum.TryParse<LogType>(dto.LogType, out var type)
                            ? type
                            : LogType.System
                    };

                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                    //dbContext.Logs.Add(log!);
                    await dbContext.SaveChangesAsync();

                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);

            Console.WriteLine("Listening...");
            await Task.Delay(Timeout.Infinite);
        }
    }
}