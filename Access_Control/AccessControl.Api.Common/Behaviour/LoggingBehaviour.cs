using AccessControl.Api.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AccessControl.Api.Common.Behaviour
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;
        private readonly RabbitMqLogProducer _logProducer;

        public LoggingBehaviour(ILogger<TRequest> logger, RabbitMqLogProducer logProducer)
        {
            _logger = logger;
            _logProducer = logProducer;
        }

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
