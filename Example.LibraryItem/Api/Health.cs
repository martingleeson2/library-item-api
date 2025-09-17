using System.Text.Json;

namespace Example.LibraryItem.Api;

public static class Health
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
    // Register health checks mapping (services added in Program.cs)

        var builder = endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = async (ctx, report) =>
            {
                ctx.Response.ContentType = "application/json";
                var payload = new
                {
                    status = report.Status.ToString().ToLowerInvariant(),
                    timestamp = DateTime.UtcNow,
                    results = report.Entries.ToDictionary(
                        e => e.Key,
                        e => new
                        {
                            status = e.Value.Status.ToString().ToLowerInvariant(),
                            description = e.Value.Description,
                            duration_ms = (int)e.Value.Duration.TotalMilliseconds
                        })
                };
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        });
        builder.WithMetadata(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(StatusCodes.Status200OK));
        builder.WithMetadata(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(StatusCodes.Status503ServiceUnavailable));
        builder.WithMetadata(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized));
        builder.WithMetadata(new Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden));
        builder.RequireAuthorization();

        return endpoints;
    }
}
