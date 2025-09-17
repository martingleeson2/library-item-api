using FluentValidation;

namespace Example.LibraryItem.Api.Interfaces;

public interface IErrorResponseWriter
{
    Task WriteValidationErrorAsync(HttpContext context, Guid requestId, ValidationException exception, CancellationToken cancellationToken = default);
    Task WriteErrorAsync(HttpContext context, Guid requestId, int statusCode, string errorCode, string message, CancellationToken cancellationToken = default);
}