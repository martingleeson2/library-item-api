using Example.LibraryItem.Api.Interfaces;
using FluentValidation;

namespace Example.LibraryItem.Api.Services;

/// <summary>
/// Implementation of exception mapping service that maps .NET exceptions to HTTP response information.
/// </summary>
public class ExceptionMappingService : IExceptionMappingService
{
    /// <summary>
    /// Maps an exception to the appropriate HTTP status code and error information.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple containing the status code, error code, and message.</returns>
    public (int StatusCode, string ErrorCode, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException => (StatusCodes.Status422UnprocessableEntity, ErrorCodes.VALIDATION_ERROR, "The request contains validation errors"),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, ErrorCodes.FORBIDDEN, "Insufficient permissions"),
            InvalidOperationException ioex => (StatusCodes.Status409Conflict, ioex.Message, ioex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, ErrorCodes.ITEM_NOT_FOUND, "The requested resource could not be found"),
            OperationCanceledException => (499, "REQUEST_CANCELLED", "Request was cancelled"),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.INTERNAL_SERVER_ERROR, "An unexpected error occurred")
        };
    }
}