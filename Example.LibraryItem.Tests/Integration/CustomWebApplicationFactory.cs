using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Example.LibraryItem.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Example.LibraryItem.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["DisableHttpsRedirection"] = "true",
                ["DisableHttpLogging"] = "true"
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
