using MediatR;
using Microsoft.Extensions.Logging;

namespace NestHub.Application.Common.Behaviors;

public sealed class UnhandledExceptionLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionLoggingBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionLoggingBehavior(ILogger<UnhandledExceptionLoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {RequestName}", typeof(TRequest).Name);
            throw;
        }
    }
}
