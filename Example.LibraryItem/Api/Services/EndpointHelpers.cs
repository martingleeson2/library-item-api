using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Application.Interfaces;

namespace Example.LibraryItem.Api.Services
{
    public class EndpointHelpers(IDateTimeProvider dateTimeProvider) : IEndpointHelpers
    {
        public string GetBasePath(HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}";
        }

        public string? GetCurrentUser(HttpContext context)
        {
            return context.User.Identity?.Name;
        }

        public ErrorResponseDto CreateNotFoundResponse(HttpContext context, string entityType = "item")
        {
            return new ErrorResponseDto
            {
                Error = entityType.ToUpperInvariant() + "_NOT_FOUND",
                Message = $"The requested library {entityType} could not be found",
                Timestamp = dateTimeProvider.UtcNow,
                RequestId = GenerateRequestId(),
                Path = context.Request.Path
            };
        }

        public ErrorResponseDto CreateBadRequestResponse(HttpContext context, string message, string? details = null)
        {
            return new ErrorResponseDto
            {
                Error = "BAD_REQUEST",
                Message = message,
                Details = details,
                Timestamp = dateTimeProvider.UtcNow,
                RequestId = GenerateRequestId(),
                Path = context.Request.Path
            };
        }

        public Guid GenerateRequestId()
        {
            return Guid.NewGuid();
        }
    }
}