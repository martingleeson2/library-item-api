using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Application;
using FluentValidation;

namespace Example.LibraryItem.Api;

public class ErrorResponseWriter(TimeProvider timeProvider) : IErrorResponseWriter
{
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task WriteValidationErrorAsync(HttpContext context, Guid requestId, ValidationException exception, CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        EnsureJsonContentType(context);

        var payload = new ValidationErrorResponseDto
        {
            error = ErrorCodes.VALIDATION_ERROR,
            message = "The request contains validation errors",
            validation_errors = exception.Errors.Select(e => new ValidationError 
            { 
                field = e.PropertyName, 
                message = e.ErrorMessage 
            }).ToList(),
            timestamp = _timeProvider.GetUtcNow().UtcDateTime,
            request_id = requestId
        };

        await context.Response.WriteAsJsonAsync(payload, cancellationToken);
    }

    public async Task WriteErrorAsync(HttpContext context, Guid requestId, int statusCode, string errorCode, string message, CancellationToken cancellationToken = default)
    {
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        EnsureJsonContentType(context);

        var payload = new ErrorResponseDto
        {
            error = errorCode,
            message = message,
            timestamp = _timeProvider.GetUtcNow().UtcDateTime,
            request_id = requestId,
            path = context.Request.Path.ToString()
        };

        await context.Response.WriteAsJsonAsync(payload, cancellationToken);
    }

    private static void EnsureJsonContentType(HttpContext context)
    {
        if (string.IsNullOrEmpty(context.Response.ContentType))
        {
            context.Response.ContentType = "application/json";
        }
    }
}