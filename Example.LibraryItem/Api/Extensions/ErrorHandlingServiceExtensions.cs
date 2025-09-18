using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Middleware;
using Example.LibraryItem.Api.Services;

namespace Example.LibraryItem.Api.Extensions;

public static class ErrorHandlingServiceExtensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<IErrorResponseWriter, ErrorResponseWriter>();
        services.AddSingleton<IRequestContextService, RequestContextService>();
        services.AddSingleton<IExceptionMappingService, ExceptionMappingService>();
        return services;
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();

    public static IApplicationBuilder UseProblemHandling(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}