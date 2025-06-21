using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using User.Api.Common.Services;
using User.Api.Infrastructure.Persistance;

namespace User.Api.Infrastructure.Services
{
    public class UserApiSaga : RabbitMqService
    {
        private const string UserToDocumentQueue = "user_to_document_queue";
        private const string DocumentToUserQueue = "document_to_user_queue";

        private readonly ConcurrentDictionary<string, SagaState> _sagaMap = new();
        private readonly UserDatabaseContext _dbContext;

        public UserApiSaga(IConfiguration configuration, UserDatabaseContext dbContext)
            : base(configuration.GetSection("RabbitMQ:Host").Value ?? "")
        {
            // Ensure the database context is not null
            _dbContext = dbContext;

            DeclareQueueAsync(UserToDocumentQueue).Wait();
            DeclareQueueAsync(DocumentToUserQueue).Wait();

            StartConsumer(DocumentToUserQueue, OnDocumentMessageReceivedAsync);
        }

        public async Task<bool> StartDeleteSagaAsync(Guid UserId, CancellationToken cancellationToken = default)
        {
            var sagaId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var state = new SagaState
            {
                UserId = UserId,
                CompletionSource = tcs
            };

            _sagaMap[sagaId] = state;

            Console.WriteLine($"[UserApiSaga] Starting saga {sagaId} for user {UserId}");

            var prepareMessage = new SagaMessage
            {
                SagaId = sagaId,
                Type = "PrepareDeleteCommand",
                Payload = JsonDocument.Parse($"{{\"UserId\":\"{UserId}\"}}").RootElement
            };

            await PublishAsync(UserToDocumentQueue, prepareMessage);
            Console.WriteLine("[UserApiSaga] Sent PrepareDeleteCommand");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                return await tcs.Task.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[UserApiSaga] Saga {sagaId} timed out");
                _sagaMap.TryRemove(sagaId, out _);
                return false;
            }
        }

        private async Task OnDocumentMessageReceivedAsync(SagaMessage message)
        {
            if (!_sagaMap.TryGetValue(message.SagaId, out var state))
            {
                Console.WriteLine($"[UserApiSaga] Ignoring unknown saga message {message.SagaId}");
                return;
            }

            Console.WriteLine($"[UserApiSaga] Received message of type {message.Type} for saga {message.SagaId}");

            switch (message.Type)
            {
                case "PrepareDeleteAcknowledged":
                    await SendDeleteCommandAsync(message.SagaId, state.UserId);
                    break;

                case "DeleteSucceeded":
                    await DeleteUserDataAsync(state.UserId);
                    state.CompletionSource.TrySetResult(true);
                    _sagaMap.TryRemove(message.SagaId, out _);
                    Console.WriteLine($"[UserApiSaga] Saga {message.SagaId} completed successfully");
                    break;

                case "DeleteFailed":
                    Console.WriteLine($"[UserApiSaga] User delete failed for saga {message.SagaId}");
                    state.CompletionSource.TrySetResult(false);
                    _sagaMap.TryRemove(message.SagaId, out _);
                    break;
            }
        }

        private async Task SendDeleteCommandAsync(string sagaId, Guid UserId)
        {
            var deleteMessage = new SagaMessage
            {
                SagaId = sagaId,
                Type = "DeleteCommand",
                Payload = JsonDocument.Parse($"{{\"UserId\":\"{UserId}\"}}").RootElement
            };

            await PublishAsync(UserToDocumentQueue, deleteMessage);
            Console.WriteLine($"[UserApiSaga] Sent DeleteCommand for saga {sagaId}");
        }

        private async Task DeleteUserDataAsync(Guid UserId)
        {
            var user = await _dbContext.Users.FindAsync(UserId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"[UserApiSaga] Deleted user data for user {UserId}");
            }
            else
            {
                Console.WriteLine($"[UserApiSaga] No user found with user ID {UserId}");
            }
        }

        private class SagaState
        {
            public Guid UserId { get; set; } = default!;
            public TaskCompletionSource<bool> CompletionSource { get; set; } = default!;
        }
    }
}
