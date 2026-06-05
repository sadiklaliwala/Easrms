using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Easrms.Application.Behaviors
{
    public class ExceptionLoggingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly ILogger<ExceptionLoggingBehavior<TRequest, TResponse>> _logger;

        public ExceptionLoggingBehavior(
            ILogger<ExceptionLoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            try
            {
                return await next(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Exception thrown while handling {RequestName}. Request: {@Request}",
                    typeof(TRequest).FullName,
                    request);

                throw;
            }
        }
    }
}