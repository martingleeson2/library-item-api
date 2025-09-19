using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Example.LibraryItem.Infrastructure;

namespace Example.LibraryItem.Tests.Integration;

public class DevelopmentWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var inMemorySettings = new Dictionary<string, string?>
            {
                ["DisableHttpsRedirection"] = "true",
                ["DisableHttpLogging"] = "true",
                ["Database:Provider"] = "inmemory",
                ["ApiKeys:0"] = "dev-key",
            };
            configBuilder.AddInMemoryCollection(inMemorySettings!);
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<LibraryDbContext>));
            services.RemoveAll(typeof(IDbContextFactory<LibraryDbContext>));

            var dbName = $"library-devtests-{Guid.NewGuid()}";
            services.AddDbContext<LibraryDbContext>(opt => opt.UseInMemoryDatabase(dbName));
        });
    }

    public HttpClient CreateDevClientWithApiKey()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-API-Key", "dev-key");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }
}
