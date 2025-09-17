namespace Example.LibraryItem.Api.Interfaces
{
    public interface IEndpointHelpers
    {
        /// <summary>
        /// Constructs the base path for the current request
        /// </summary>
        string GetBasePath(HttpContext context);

        /// <summary>
        /// Extracts the current user identity from the HTTP context
        /// </summary>
        string? GetCurrentUser(HttpContext context);

        /// <summary>
        /// Creates a standardized 404 Not Found error response
        /// </summary>
        ErrorResponseDto CreateNotFoundResponse(HttpContext context, string entityType = "item");

        /// <summary>
        /// Creates a standardized 400 Bad Request error response
        /// </summary>
        ErrorResponseDto CreateBadRequestResponse(HttpContext context, string message, string? details = null);

        /// <summary>
        /// Generates correlation ID for the request
        /// </summary>
        Guid GenerateRequestId();
    }
}