using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;

namespace Example.LibraryItem.Api.Authentication
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiKeyAuthenticationOptions>(ApiKeyDefaults.Scheme, configuration.GetSection(ApiKeyDefaults.OptionsSection));

            services.PostConfigure<ApiKeyAuthenticationOptions>(opt =>
            {
                opt.ValidApiKeys = [.. (opt.ValidApiKeys ?? [])
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .Distinct()];
            });

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = ApiKeyDefaults.Scheme;
                })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyDefaults.Scheme, _ => { });

            services.AddAuthorization();
            return services;
        }
    }
}
