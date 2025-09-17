using Example.LibraryItem.Api.Interfaces;
using FluentValidation;

namespace Example.LibraryItem.Api.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IErrorResponseWriter errorWriter)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;
    private readonly IErrorResponseWriter _errorWriter = errorWriter;

    public async Task InvokeAsync(HttpContext context)
    {
        var cancellationToken = context.RequestAborted;
        var requestId = GetRequestId(context);
        using var scope = CreateLoggingScope(context, requestId);

        try
        {
            await _next(context);
        }
        catch (ValidationException vex)
        {
            _logger.LogWarning(vex, "Validation failure for request {RequestId}", requestId);
            await HandleExceptionAsync(context, requestId, () => 
                _errorWriter.WriteValidationErrorAsync(context, requestId, vex, cancellationToken), cancellationToken);
        }
        catch (UnauthorizedAccessException uae)
        {
            _logger.LogWarning(uae, "Forbidden access for request {RequestId}", requestId);
            await HandleExceptionAsync(context, requestId, () => 
                _errorWriter.WriteErrorAsync(context, requestId, StatusCodes.Status403Forbidden, ErrorCodes.FORBIDDEN, "Insufficient permissions", cancellationToken), cancellationToken);
        }
        catch (InvalidOperationException ioex)
        {
            _logger.LogWarning(ioex, "Conflict for request {RequestId}: {Message}", requestId, ioex.Message);
            await HandleExceptionAsync(context, requestId, () => 
                _errorWriter.WriteErrorAsync(context, requestId, StatusCodes.Status409Conflict, ioex.Message, ioex.Message, cancellationToken), cancellationToken);
        }
        catch (KeyNotFoundException knf)
        {
            _logger.LogInformation(knf, "Resource not found for request {RequestId}", requestId);
            await HandleExceptionAsync(context, requestId, () => 
                _errorWriter.WriteErrorAsync(context, requestId, StatusCodes.Status404NotFound, ErrorCodes.ITEM_NOT_FOUND, "The requested resource could not be found", cancellationToken), cancellationToken);
        }
        catch (OperationCanceledException oce) when (cancellationToken.IsCancellationRequested || context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(oce, "Request {RequestId} was cancelled", requestId);
            context.Response.StatusCode = 499; // Client Closed Request (non-standard)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestId}", requestId);
            await HandleExceptionAsync(context, requestId, () => 
                _errorWriter.WriteErrorAsync(context, requestId, StatusCodes.Status500InternalServerError, ErrorCodes.INTERNAL_SERVER_ERROR, "An unexpected error occurred", cancellationToken), cancellationToken);
        }
    }

    private static Guid GetRequestId(HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.RequestIdItemKey, out var ridObj) && ridObj is Guid g 
            ? g 
            : Guid.NewGuid();
    }

    private IDisposable CreateLoggingScope(HttpContext context, Guid requestId)
    {
        return _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Path"] = context.Request.Path.ToString(),
            ["Method"] = context.Request.Method
        })!;
    }

    private async Task HandleExceptionAsync(HttpContext context, Guid requestId, Func<Task> writeErrorAction, CancellationToken cancellationToken)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started for request {RequestId}; cannot write error response", requestId);
            return;
        }

        try
        {
            await writeErrorAction();
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Failed to write error response for request {RequestId}", requestId);
        }
    }
}