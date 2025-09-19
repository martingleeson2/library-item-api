// Authentication endpoints intentionally removed.
// This file left in place as a no-op to avoid build-time missing-file errors.
namespace Example.LibraryItem.Api
{
    public static class AuthenticationEndpoints
    {
        public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            return endpoints;
        }
    }
}