using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Middleware;

namespace Example.LibraryItem.Api.Services;

/// <summary>
/// Implementation of request context service for managing correlation IDs and request context.
/// </summary>
public class RequestContextService : IRequestContextService
{
    /// <summary>
    /// Extracts the request ID from the HttpContext, either from the correlation middleware
    /// or generates a new one if not available.
    /// </summary>
    /// <param name="context">The HTTP context containing request information.</param>
    /// <returns>The request ID for the current request.</returns>
    public Guid GetRequestId(HttpContext context)
    {
        return context.Items.TryGetValue(CorrelationIdMiddleware.RequestIdItemKey, out var ridObj) && ridObj is Guid g 
            ? g 
            : Guid.NewGuid();
    }
}