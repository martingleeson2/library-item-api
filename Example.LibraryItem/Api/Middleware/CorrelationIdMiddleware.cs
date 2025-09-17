namespace Example.LibraryItem.Api.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Request-ID";
    public const string RequestIdItemKey = "RequestId";
    
    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = ExtractOrGenerateRequestId(context);
        context.Items[RequestIdItemKey] = requestId;
        context.Response.Headers[HeaderName] = requestId.ToString();
        
        using (_logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = requestId }))
        {
            await _next(context);
        }
    }

    private static Guid ExtractOrGenerateRequestId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var values) && 
            Guid.TryParse(values.FirstOrDefault(), out var parsed))
        {
            return parsed;
        }
        
        return Guid.NewGuid();
    }
}