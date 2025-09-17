using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Example.LibraryItem.Api.Authentication
{
    public class ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyDefaults.HeaderName, out var values))
            {
                Logger.LogWarning("API request missing {Header} from {RemoteIpAddress}", ApiKeyDefaults.HeaderName, Request.HttpContext.Connection.RemoteIpAddress);
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var apiKey = values.ToString();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Logger.LogWarning("API request with empty {Header} from {RemoteIpAddress}", ApiKeyDefaults.HeaderName, Request.HttpContext.Connection.RemoteIpAddress);
                return Task.FromResult(AuthenticateResult.Fail("Missing API Key"));
            }

            var validApiKeys = Options.ValidApiKeys ?? new List<string>();

            if (validApiKeys.Count == 0)
            {
                Logger.LogError("No valid API keys configured in {ConfigPath}", ApiKeyDefaults.OptionsSection + ":ValidApiKeys");
                return Task.FromResult(AuthenticateResult.Fail("Authentication configuration error"));
            }

            if (!validApiKeys.Contains(apiKey))
            {
                Logger.LogWarning("Invalid API key attempt from {RemoteIpAddress}: {ApiKeyPrefix}...",
                    Request.HttpContext.Connection.RemoteIpAddress,
                    apiKey.Length > 8 ? apiKey.Substring(0, 8) : apiKey);
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            Logger.LogInformation("Successful API key authentication from {RemoteIpAddress}", Request.HttpContext.Connection.RemoteIpAddress);

            var identity = new ClaimsIdentity(ApiKeyDefaults.Scheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, "apikey-user"));
            identity.AddClaim(new Claim("api_key_prefix", apiKey.Length > 8 ? apiKey.Substring(0, 8) : apiKey));
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, ApiKeyDefaults.Scheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
