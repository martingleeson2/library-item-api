using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Example.LibraryItem.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example.LibraryItem.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// JSON serializer options that match the server configuration (snake_case naming policy)
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
    };
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["DisableHttpsRedirection"] = "true",
                ["DisableHttpLogging"] = "true",
                ["Database:Provider"] = "inmemory",
                // Configure API keys for tests using simplified structure
                ["ApiKeys:0"] = "test-key",
                ["ApiKeys:1"] = "integration-test-key"
            };
            configBuilder.AddInMemoryCollection(inMemorySettings!);
        });
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            services.RemoveAll(typeof(DbContextOptions<LibraryDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<LibraryDbContext>));

            // Use a unique in-memory database per factory instance to isolate tests
            var dbName = $"library-tests-{Guid.NewGuid()}";
            services.AddDbContext<LibraryDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        });
    }

    public HttpClient CreateClientWithApiKey()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "test-key");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}
