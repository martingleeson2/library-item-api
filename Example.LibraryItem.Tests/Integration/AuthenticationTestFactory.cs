using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Example.LibraryItem.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Example.LibraryItem.Tests.Integration;

/// <summary>
/// Configurable web application factory for testing different authentication mechanisms.
/// Each authentication scenario gets its own isolated configuration and database.
/// </summary>
public class AuthenticationTestFactory : WebApplicationFactory<Program>
{
    private readonly string[] _validApiKeys;
    private readonly string _dbName;

    public AuthenticationTestFactory(string[] validApiKeys)
    {
        _validApiKeys = validApiKeys;
        _dbName = $"auth-test-{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Create configuration dictionary with simplified API key structure
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["DisableHttpsRedirection"] = "true",
                ["DisableHttpLogging"] = "true",
                ["Database:Provider"] = "inmemory",
                ["Jwt:Enabled"] = "true",
                ["Jwt:SecretKey"] = "LibraryManagementSecretKey2024!@#$%^&*()ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890",
                ["Jwt:Issuer"] = "library-management-api",
                ["Jwt:Audience"] = "library-users",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            // Add API keys
            for (int i = 0; i < _validApiKeys.Length; i++)
            {
                inMemorySettings[$"ApiKeys:{i}"] = _validApiKeys[i];
            }

            configBuilder.AddInMemoryCollection(inMemorySettings!);
        });

        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            services.RemoveAll(typeof(DbContextOptions<LibraryDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<LibraryDbContext>));

            // Use unique in-memory database per test instance
            services.AddDbContext<LibraryDbContext>(opt => opt.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>
    /// Creates an HTTP client with the specified API key header.
    /// </summary>
    public HttpClient CreateClientWithApiKey(string apiKey, string headerName = "X-API-Key")
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add(headerName, apiKey);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    /// <summary>
    /// Creates an HTTP client without any authentication headers.
    /// </summary>
    public HttpClient CreateUnauthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}