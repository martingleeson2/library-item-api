namespace Example.LibraryItem.Api.Interfaces;

/// <summary>
/// Service for mapping exceptions to appropriate HTTP responses.
/// </summary>
public interface IExceptionMappingService
{
    /// <summary>
    /// Maps an exception to the appropriate HTTP status code and error information.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple containing the status code, error code, and message.</returns>
    (int StatusCode, string ErrorCode, string Message) MapException(Exception exception);
}