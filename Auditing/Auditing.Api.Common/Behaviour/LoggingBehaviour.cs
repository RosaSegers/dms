using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auditing.Api.Common.Behaviour
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<TRequest> _logger;

        public LoggingBehaviour(ILogger<TRequest> logger)
        {
            _logger = logger;
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
                _logger.LogInformation(
                    $"[END] {requestNameWithGuid}; Execution time = {stopwatch.Elapsed}");
            }

            return response;
        }
    }
}
