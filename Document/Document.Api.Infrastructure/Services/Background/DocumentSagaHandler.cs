using Document.Api.Common.Services;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace Document.Api.Infrastructure.Services
{
    public class DocumentApiSaga : BackgroundService
    {
        private const string UserToDocumentQueue = "user_to_document_queue";
        private const string DocumentToUserQueue = "document_to_user_queue";

        private readonly ISender _mediator;
        private readonly RabbitMqService _rabbitMq;

        public DocumentApiSaga(IConfiguration configuration, ISender mediator)
        {
            _mediator = mediator;
            var hostname = configuration.GetSection("RabbitMQ:Host").Value ?? "";
            _rabbitMq = new RabbitMqService(hostname);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _rabbitMq.DeclareQueueAsync(UserToDocumentQueue);
            await _rabbitMq.DeclareQueueAsync(DocumentToUserQueue);

            await _rabbitMq.StartConsumer(UserToDocumentQueue, OnUserMessageReceivedAsync);

            Console.WriteLine("[DocumentApiSaga] Listening for saga messages...");

            // Keep alive until shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task OnUserMessageReceivedAsync(SagaMessage message)
        {
            Console.WriteLine($"[DocumentApiSaga] Received message type: {message.Type}");

            switch (message.Type)
            {
                case "PrepareDeleteCommand":
                    await HandlePrepareDeleteAsync(message);
                    break;

                case "DeleteCommand":
                    await HandleDeleteAsync(message);
                    break;
            }
        }

        private async Task HandlePrepareDeleteAsync(SagaMessage message)
        {
            var userId = message.Payload.GetProperty("UserId").GetString();
            Console.WriteLine($"[DocumentApiSaga] Preparing to delete documents for user {userId}");

            var existsQuery = new ExistsDocumentByUserIdQuery(Guid.Parse(userId));
            var existsResult = await _mediator.Send(existsQuery);

            if (!existsResult.Value)
            {
                Console.WriteLine($"[DocumentApiSaga] No documents found for user {userId}");
            }

            var ackMessage = new SagaMessage
            {
                SagaId = message.SagaId,
                Type = "PrepareDeleteAcknowledged",
                Payload = JsonDocument.Parse($"{{\"UserId\":\"{userId}\"}}").RootElement
            };

            await _rabbitMq.PublishAsync(DocumentToUserQueue, ackMessage);
            Console.WriteLine("[DocumentApiSaga] Sent PrepareDeleteAcknowledged");
        }

        private async Task HandleDeleteAsync(SagaMessage message)
        {
            var userId = message.Payload.GetProperty("UserId").GetString();
            Console.WriteLine($"[DocumentApiSaga] Deleting documents for user {userId}");

            var command = new DeleteDocumentByUserIdCommand(Guid.Parse(userId));
            var result = await _mediator.Send(command);

            SagaMessage response = new()
            {
                SagaId = message.SagaId,
                Type = result.IsError ? "DeleteFailed" : "DeleteSucceeded",
                Payload = JsonDocument.Parse($"{{\"UserId\":\"{userId}\"}}").RootElement
            };

            Console.WriteLine($"[DocumentApiSaga] Deletion {(result.IsError ? "failed" : "succeeded")} for user {userId}");
            await _rabbitMq.PublishAsync(DocumentToUserQueue, response);
        }

        public record DeleteDocumentByUserIdCommand(Guid Id) : IRequest<ErrorOr<Unit>>;
        public record ExistsDocumentByUserIdQuery(Guid Id) : IRequest<ErrorOr<bool>>;
    }
}
