using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.LibraryItem.Api;

public static class ApiKeyValidator
{
    private static readonly HashSet<string> PlaceholderKeys = [
        "CHANGE-ME-OR-SERVER-WILL-CRASH",
        "REPLACE-THIS-KEY-BEFORE-PRODUCTION",
        "dev-key",
        "test-key",
        "local-development-key"
    ];

    /// <summary>
    /// Validates API key configuration on startup and crashes if placeholder keys are detected in non-Development environments.
    /// </summary>
    /// <param name="services">Service provider for dependency injection</param>
    /// <param name="environment">Web host environment</param>
    /// <exception cref="InvalidOperationException">Thrown when invalid configuration is detected</exception>
    public static void ValidateOrCrash(IServiceProvider services, IWebHostEnvironment environment)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        var validApiKeys = configuration.GetSection("ApiKeys").Get<string[]>() ?? [];

        if (validApiKeys.Length == 0)
        {
            logger.LogCritical("FATAL: No valid API keys configured in ApiKeys section. Server cannot start without authentication");
            throw new InvalidOperationException("FATAL: No valid API keys configured in ApiKeys section. Server cannot start without authentication.");
        }

        if (environment.IsDevelopment())
        {
            logger.LogInformation("API key validation: Development environment. {Count} key(s) configured.", validApiKeys.Length);
            return;
        }

        // Skip placeholder validation for Testing environment
        if (environment.EnvironmentName == "Testing")
        {
            logger.LogInformation("API key validation: Testing environment. {Count} key(s) configured.", validApiKeys.Length);
            return;
        }

        var offending = validApiKeys.Where(k => PlaceholderKeys.Contains(k)).ToList();
        if (offending.Count != 0)
        {
            var placeholderList = string.Join(", ", offending);
            logger.LogCritical(
                "FATAL: Placeholder API keys detected in {Environment}. Found: [{PlaceholderKeys}]. Replace with secure production keys before deploying.",
                environment.EnvironmentName,
                placeholderList);
            throw new InvalidOperationException(
                $"FATAL: Placeholder API keys detected in {environment.EnvironmentName}. Found: [{placeholderList}]. Replace with secure production keys before deploying.");
        }

        logger.LogInformation("API key validation passed for {Environment}. {Count} key(s) configured.", environment.EnvironmentName, validApiKeys.Length);
    }
}