using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Services;

namespace Example.LibraryItem.Api.Extensions;

/// <summary>
/// Extension methods for configuring services in the dependency injection container.
/// Provides centralized registration of error handling and related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all error handling services required for the application.
    /// This includes error response writing, request context tracking, and exception mapping.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<IErrorResponseWriter, ErrorResponseWriter>();
        services.AddSingleton<IRequestContextService, RequestContextService>();
        services.AddSingleton<IExceptionMappingService, ExceptionMappingService>();
        return services;
    }
}