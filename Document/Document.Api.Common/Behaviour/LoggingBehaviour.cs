using Document.Api.Common.Interfaces;
using Document.Api.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Document.Api.Common.Behaviour
{
    public class LoggingBehaviour<TRequest, TResponse>
        (ILogger<TRequest> logger, RabbitMqLogProducer logProducer, ICurrentUserService userService) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger = logger;
        private readonly RabbitMqLogProducer _logProducer = logProducer;
        private readonly ICurrentUserService _userService = userService;


        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = request.GetType().Name;
            var requestId = Guid.NewGuid().ToString();
            var requestNameWithGuid = $"{requestName} [{requestId}]";

            _logger.LogInformation($"[START] {requestNameWithGuid}");
            TResponse response;
            var stopwatch = Stopwatch.StartNew();

            try
            {
                response = await next();
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"[END] {requestNameWithGuid}; Execution time = {stopwatch.Elapsed}");

                // Create and send log to RabbitMQ
                var log = new
                {
                    _userService.UserId,
                    Message = $"Request {requestNameWithGuid} completed in {stopwatch.Elapsed}",
                    RequestName = requestName,
                    RequestId = requestId,
                    Severity = "Information",
                    Metadata = $"ExecutionTime: {stopwatch.Elapsed}"
                };

                _logProducer.PublishLog(log);
            }

            return response;
        }
    }
}
