using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Example.LibraryItem.Api;

public static class ApiKeyValidator
{
    private const string ValidApiKeysConfigPath = "Authentication:ValidApiKeys";

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

        var validApiKeys = GetConfiguredApiKeys(configuration);

        if (validApiKeys.Count == 0)
        {
            logger.LogCritical("FATAL: No API keys configured in {ConfigPath}. Server cannot start without authentication", ValidApiKeysConfigPath);
            throw new InvalidOperationException($"FATAL: No API keys configured in {ValidApiKeysConfigPath}. Server cannot start without authentication.");
        }

        if (environment.IsDevelopment())
        {
            logger.LogInformation("API key validation: Development environment. {Count} key(s) configured.", validApiKeys.Count);
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

        logger.LogInformation("API key validation passed for {Environment}. {Count} key(s) configured.", environment.EnvironmentName, validApiKeys.Count);
    }

    private static List<string> GetConfiguredApiKeys(IConfiguration configuration)
    {
        var keys = configuration.GetSection(ValidApiKeysConfigPath).Get<string[]>() ?? [];
        
        return [.. keys
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct()];
    }
}