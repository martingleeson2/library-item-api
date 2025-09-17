using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Middleware;

namespace Example.LibraryItem.Api.Extensions;

public static class ErrorHandlingServiceExtensions
{
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<IErrorResponseWriter, ErrorResponseWriter>();
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