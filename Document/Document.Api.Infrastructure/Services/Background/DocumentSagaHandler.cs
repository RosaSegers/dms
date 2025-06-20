﻿using Document.Api.Common.Services;
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
            try
            {
                Console.WriteLine($"[DocumentApiSaga] Handling PrepareDeleteCommand for saga {message.SagaId}");

                var UserId = Guid.Parse(message.Payload.GetProperty("UserId").GetString()!);
                Console.WriteLine($"[DocumentApiSaga] Preparing to delete documents for user {UserId}");

                var existsQuery = new ExistsDocumentByUserIdQuery(UserId);
                var existsResult = await _mediator.Send(existsQuery);

                Console.WriteLine($"ExistsDocumentByUserIdQuery result: {existsResult.IsError} / {existsResult.Value}");

                if (!existsResult.Value)
                {
                    Console.WriteLine($"[DocumentApiSaga] No documents found for user {UserId}");
                }

                var ackMessage = new SagaMessage
                {
                    SagaId = message.SagaId,
                    Type = "PrepareDeleteAcknowledged",
                    Payload = JsonDocument.Parse($"{{\"UserId\":\"{UserId}\"}}").RootElement
                };

                await _rabbitMq.PublishAsync(DocumentToUserQueue, ackMessage);
                Console.WriteLine("[DocumentApiSaga] Sent PrepareDeleteAcknowledged");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DocumentApiSaga] Error handling PrepareDeleteCommand: {ex.Message}");
                return;
            }
            finally
            {
                var userId = message.Payload.GetProperty("UserId").GetString();

                var ackMessage = new SagaMessage
                {
                    SagaId = message.SagaId,
                    Type = "PrepareDeleteAcknowledged",
                    Payload = JsonDocument.Parse($"{{\"UserId\":\"{userId}\"}}").RootElement
                };

                await _rabbitMq.PublishAsync(DocumentToUserQueue, ackMessage);
                Console.WriteLine("[DocumentApiSaga] Sent PrepareDeleteAcknowledged");
            }
        }

        private async Task HandleDeleteAsync(SagaMessage message)
        {
            try
            {
                var UserId = Guid.Parse(message.Payload.GetProperty("UserId").GetString()!);
                Console.WriteLine($"[DocumentApiSaga] Deleting documents for user {UserId}");

                var command = new DeleteDocumentByUserIdCommand(UserId);
                Console.WriteLine($"[DocumentApiSaga] Sending DeleteDocumentByUserIdCommand for user {UserId}");

                var result = await _mediator.Send(command);
                Console.WriteLine($"DeleteDocumentByUserIdCommand result: {result.IsError} / {result.Value}");

                SagaMessage response = new()
                {
                    SagaId = message.SagaId,
                    Type = result.IsError ? "DeleteFailed" : "DeleteSucceeded",
                    Payload = JsonDocument.Parse($"{{\"UserId\":\"{UserId}\"}}").RootElement
                };

                Console.WriteLine($"[DocumentApiSaga] Deletion {(result.IsError ? "failed" : "succeeded")} for user {UserId}");
                await _rabbitMq.PublishAsync(DocumentToUserQueue, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DocumentApiSaga] Error handling DeleteCommand: {ex.Message}");
            }
            finally
            {
                var errorResponse = new SagaMessage
                {
                    SagaId = message.SagaId,
                    Type = "DeleteFailed",
                    Payload = JsonDocument.Parse($"{{\"UserId\":\"{message.Payload.GetProperty("UserId").GetString()}\"}}").RootElement
                };
                await _rabbitMq.PublishAsync(DocumentToUserQueue, errorResponse);
            }
        }

        public record DeleteDocumentByUserIdCommand(Guid UserId) : IRequest<ErrorOr<Unit>>;
        public record ExistsDocumentByUserIdQuery(Guid UserId) : IRequest<ErrorOr<bool>>;
    }
}
