using Example.LibraryItem.Api.Interfaces;
using FluentValidation;

namespace Example.LibraryItem.Api.Middleware;

/// <summary>
/// Middleware for handling unhandled exceptions and converting them to appropriate HTTP responses.
/// This middleware catches exceptions from downstream middleware and handlers, maps them to 
/// appropriate HTTP status codes and error responses, and ensures consistent error handling
/// across the application.
/// </summary>
public class GlobalExceptionMiddleware(
    RequestDelegate next, 
    ILogger<GlobalExceptionMiddleware> logger, 
    IErrorResponseWriter errorWriter,
    IRequestContextService requestContextService,
    IExceptionMappingService exceptionMappingService)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;
    private readonly IErrorResponseWriter _errorWriter = errorWriter;
    private readonly IRequestContextService _requestContextService = requestContextService;
    private readonly IExceptionMappingService _exceptionMappingService = exceptionMappingService;

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and catch any unhandled exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;
        var requestId = _requestContextService.GetRequestId(context);
        using var scope = CreateLoggingScope(context, requestId);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, requestId, ex, cancellationToken);
        }
    }

    /// <summary>
    /// Handles the exception by logging it and writing an appropriate error response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="requestId">The correlation ID for the request.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    private async Task HandleExceptionAsync(HttpContext context, Guid requestId, Exception exception, CancellationToken cancellationToken)
    {
        LogException(exception, requestId);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started for request {RequestId}; cannot write error response", requestId);
            return;
        }

        try
        {
            await WriteErrorResponseAsync(context, requestId, exception, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Don't log cancellation exceptions during error writing
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write error response for request {RequestId}", requestId);
        }
    }

    /// <summary>
    /// Logs the exception with appropriate level and message based on exception type.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="requestId">The correlation ID for the request.</param>
    private void LogException(Exception exception, Guid requestId)
    {
        switch (exception)
        {
            case ValidationException vex:
                _logger.LogWarning(vex, "Validation failure for request {RequestId}", requestId);
                break;
            case UnauthorizedAccessException uae:
                _logger.LogWarning(uae, "Forbidden access for request {RequestId}", requestId);
                break;
            case InvalidOperationException ioex:
                _logger.LogWarning(ioex, "Conflict for request {RequestId}: {Message}", requestId, ioex.Message);
                break;
            case KeyNotFoundException knf:
                _logger.LogInformation(knf, "Resource not found for request {RequestId}", requestId);
                break;
            case OperationCanceledException oce when oce.CancellationToken.IsCancellationRequested:
                _logger.LogInformation(oce, "Request {RequestId} was cancelled", requestId);
                break;
            default:
                _logger.LogError(exception, "Unhandled exception for request {RequestId}", requestId);
                break;
        }
    }

    /// <summary>
    /// Writes the appropriate error response based on the exception type.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="requestId">The correlation ID for the request.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    private async Task WriteErrorResponseAsync(HttpContext context, Guid requestId, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException vex)
        {
            await _errorWriter.WriteValidationErrorAsync(context, requestId, vex, cancellationToken);
            return;
        }

        if (exception is OperationCanceledException oce && oce.CancellationToken.IsCancellationRequested)
        {
            context.Response.StatusCode = 499; // Client Closed Request (non-standard)
            return;
        }

        var (statusCode, errorCode, message) = _exceptionMappingService.MapException(exception);
        await _errorWriter.WriteErrorAsync(context, requestId, statusCode, errorCode, message, cancellationToken);
    }

    /// <summary>
    /// Creates a logging scope with request context information.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="requestId">The correlation ID for the request.</param>
    /// <returns>A disposable logging scope.</returns>
    private IDisposable CreateLoggingScope(HttpContext context, Guid requestId)
    {
        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        })!;
    }
}