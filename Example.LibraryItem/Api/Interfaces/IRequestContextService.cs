namespace Example.LibraryItem.Api.Interfaces;

/// <summary>
/// Service for managing request context information, particularly correlation IDs.
/// </summary>
public interface IRequestContextService
{
    /// <summary>
    /// Extracts the request ID from the HttpContext, either from the correlation middleware
    /// or generates a new one if not available.
    /// </summary>
    /// <param name="context">The HTTP context containing request information.</param>
    /// <returns>The request ID for the current request.</returns>
    Guid GetRequestId(HttpContext context);
}