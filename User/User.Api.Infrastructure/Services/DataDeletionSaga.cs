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
            // Initialize RabbitMQ queues and consumers
            Console.WriteLine("[UserApiSaga] Initializing queues and consumers");

            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext), "UserDatabaseContext must not be null");

            DeclareQueueAsync(UserToDocumentQueue).Wait();
            Console.WriteLine($"[UserApiSaga] Declared queue '{UserToDocumentQueue}'");

            DeclareQueueAsync(DocumentToUserQueue).Wait();
            Console.WriteLine($"[UserApiSaga] Declared queue '{DocumentToUserQueue}'");

            StartConsumer(DocumentToUserQueue, OnDocumentMessageReceivedAsync);
            Console.WriteLine($"[UserApiSaga] Started consumer for queue '{DocumentToUserQueue}'");
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
            Console.WriteLine($"[UserApiSaga] Sent PrepareDeleteCommand for saga {sagaId} to queue '{UserToDocumentQueue}'");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(15));

            try
            {
                var result = await tcs.Task.WaitAsync(timeoutCts.Token);
                Console.WriteLine($"[UserApiSaga] Saga {sagaId} completed with result: {result}");
                return result;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[UserApiSaga] TIMEOUT: Saga {sagaId} did not complete within timeout.");
                _sagaMap.TryRemove(sagaId, out _);
                return false;
            }
        }

        private async Task OnDocumentMessageReceivedAsync(SagaMessage message)
        {
            Console.WriteLine($"[UserApiSaga] Received message: Type={message.Type}, SagaId={message.SagaId}");

            if (!_sagaMap.TryGetValue(message.SagaId, out var state))
            {
                Console.WriteLine($"[UserApiSaga] WARNING: Ignoring unknown saga message with SagaId={message.SagaId}");
                return;
            }

            Console.WriteLine($"[UserApiSaga] Processing message '{message.Type}' for user {state.UserId}");

            switch (message.Type)
            {
                case "PrepareDeleteAcknowledged":
                    Console.WriteLine($"[UserApiSaga] PrepareDeleteAcknowledged received. Proceeding to send DeleteCommand.");
                    await SendDeleteCommandAsync(message.SagaId, state.UserId);
                    break;

                case "DeleteSucceeded":
                    Console.WriteLine($"[UserApiSaga] DeleteSucceeded received. Proceeding to delete user data.");
                    await DeleteUserDataAsync(state.UserId);
                    state.CompletionSource.TrySetResult(true);
                    _sagaMap.TryRemove(message.SagaId, out _);
                    Console.WriteLine($"[UserApiSaga] Saga {message.SagaId} marked as completed successfully.");
                    break;

                case "DeleteFailed":
                    Console.WriteLine($"[UserApiSaga] ERROR: Delete failed for user {state.UserId} (SagaId={message.SagaId})");
                    state.CompletionSource.TrySetResult(false);
                    _sagaMap.TryRemove(message.SagaId, out _);
                    break;

                default:
                    Console.WriteLine($"[UserApiSaga] WARNING: Unknown message type '{message.Type}' received for SagaId={message.SagaId}");
                    break;
            }
        }

        private async Task SendDeleteCommandAsync(string sagaId, Guid UserId)
        {
            Console.WriteLine($"[UserApiSaga] Sending DeleteCommand for SagaId={sagaId}, UserId={UserId}");

            var deleteMessage = new SagaMessage
            {
                SagaId = sagaId,
                Type = "DeleteCommand",
                Payload = JsonDocument.Parse($"{{\"UserId\":\"{UserId}\"}}").RootElement
            };

            await PublishAsync(UserToDocumentQueue, deleteMessage);
            Console.WriteLine($"[UserApiSaga] DeleteCommand published to '{UserToDocumentQueue}' for SagaId={sagaId}");
        }

        private async Task DeleteUserDataAsync(Guid UserId)
        {
            Console.WriteLine($"[UserApiSaga] Attempting to delete user data for UserId={UserId}");

            var user = await _dbContext.Users.FindAsync(UserId);
            if (user != null)
            {
                Console.WriteLine($"[UserApiSaga] Found user {UserId}. Proceeding with deletion.");
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                Console.WriteLine($"[UserApiSaga] Successfully deleted user data for UserId={UserId}");
            }
            else
            {
                Console.WriteLine($"[UserApiSaga] WARNING: No user found with UserId={UserId}. Nothing to delete.");
            }
        }

        private class SagaState
        {
            public Guid UserId { get; set; } = default!;
            public TaskCompletionSource<bool> CompletionSource { get; set; } = default!;
        }
    }
}
