using Example.LibraryItem.Api.Middleware;

namespace Example.LibraryItem.Api.Extensions;

/// <summary>
/// Extension methods for configuring the application request pipeline.
/// Provides centralized middleware registration for error handling and request tracking.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds correlation ID middleware to the request pipeline.
    /// This middleware ensures each request has a unique correlation ID for tracking and logging.
    /// </summary>
    /// <param name="app">The application builder to configure</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    /// <summary>
    /// Adds global exception handling middleware to the request pipeline.
    /// This middleware catches unhandled exceptions and converts them to appropriate HTTP responses.
    /// </summary>
    /// <param name="app">The application builder to configure</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseProblemHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}