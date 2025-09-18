using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Example.LibraryItem.Api.Authentication
{
    /// <summary>
    /// Handles API key authentication for HTTP requests.
    /// Supports extracting API keys from both headers and query parameters,
    /// validates them against configured valid keys, and creates authentication tickets.
    /// </summary>
    public class ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : AuthenticationHandler<ApiKeyAuthenticationOptions>(options, logger, encoder)
    {
        /// <summary>
        /// Length of API key prefix to include in logs for security auditing.
        /// This provides enough context for debugging while not exposing the full key.
        /// </summary>
        private const int API_KEY_PREFIX_LENGTH = 8;
        /// <summary>
        /// Performs the core authentication logic for API key validation.
        /// Attempts to extract, validate, and authenticate requests using API keys.
        /// </summary>
        /// <returns>Authentication result indicating success, failure, or no result</returns>
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            try
            {
                // Step 1: Extract API key from request (header or query parameter)
                var apiKey = ExtractApiKey();
                if (apiKey == null)
                {
                    return Task.FromResult(CreateNoResultForMissingKey());
                }

                // Step 2: Validate API key is not empty or whitespace
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return Task.FromResult(CreateFailureForEmptyKey());
                }

                // Step 3: Validate API key against configured valid keys
                var validationResult = ValidateApiKey(apiKey);
                if (!validationResult.IsValid)
                {
                    return Task.FromResult(validationResult.Result);
                }

                // Step 4: Log successful authentication and create ticket
                Logger.LogInformation("Successful API key authentication from {RemoteIpAddress}", 
                    GetRemoteIpAddress());

                var ticket = CreateAuthenticationTicket(apiKey);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                // Handle any unexpected errors during authentication
                Logger.LogError(ex, "Error during API key authentication from {RemoteIpAddress}", 
                    GetRemoteIpAddress());
                return Task.FromResult(AuthenticateResult.Fail("Authentication error"));
            }
        }

        /// <summary>
        /// Extracts API key from the HTTP request.
        /// First checks the configured header, then falls back to query parameter if enabled.
        /// </summary>
        /// <returns>The API key string if found, null otherwise</returns>
        private string? ExtractApiKey()
        {
            // Try header first
            if (Request.Headers.TryGetValue(Options.HeaderName, out var headerValues))
            {
                return headerValues.ToString();
            }

            // Try query parameter if allowed
            if (Options.AllowQueryParameter && 
                Request.Query.TryGetValue(Options.QueryParameterName, out var queryValues))
            {
                return queryValues.ToString();
            }

            return null;
        }

        /// <summary>
        /// Creates an authentication no-result response for missing API key.
        /// Logs the attempt for security monitoring.
        /// </summary>
        /// <returns>Authentication result indicating no authentication was attempted</returns>
        private AuthenticateResult CreateNoResultForMissingKey()
        {
            Logger.LogWarning("API request missing {Header} from {RemoteIpAddress}", 
                Options.HeaderName, GetRemoteIpAddress());
            return AuthenticateResult.NoResult();
        }

        /// <summary>
        /// Creates an authentication failure response for empty/whitespace API key.
        /// Logs the attempt for security monitoring.
        /// </summary>
        /// <returns>Authentication failure result</returns>
        private AuthenticateResult CreateFailureForEmptyKey()
        {
            Logger.LogWarning("API request with empty {Header} from {RemoteIpAddress}", 
                Options.HeaderName, GetRemoteIpAddress());
            return AuthenticateResult.Fail("Missing API Key");
        }

        /// <summary>
        /// Validates the provided API key against the configured list of valid keys.
        /// Checks for configuration errors and logs invalid attempts for security monitoring.
        /// </summary>
        /// <param name="apiKey">The API key to validate</param>
        /// <returns>Tuple indicating validity and authentication result if invalid</returns>
        private (bool IsValid, AuthenticateResult Result) ValidateApiKey(string apiKey)
        {
            var validApiKeys = Options.ValidApiKeys ?? [];

            // Check if any valid API keys are configured
            if (validApiKeys.Count == 0)
            {
                Logger.LogError("No valid API keys configured in authentication options");
                return (false, AuthenticateResult.Fail("Authentication configuration error"));
            }

            // Check if the provided API key is in the valid list
            if (!validApiKeys.Contains(apiKey))
            {
                Logger.LogWarning("Invalid API key attempt from {RemoteIpAddress}: {ApiKeyPrefix}...",
                    GetRemoteIpAddress(), GetApiKeyPrefix(apiKey));
                return (false, AuthenticateResult.Fail("Invalid API Key"));
            }

            return (true, AuthenticateResult.NoResult()); // Result not used when valid
        }

        /// <summary>
        /// Creates an authentication ticket for a successfully validated API key.
        /// Includes standard claims and API key prefix for auditing purposes.
        /// </summary>
        /// <param name="apiKey">The validated API key</param>
        /// <returns>Authentication ticket containing user identity and claims</returns>
        private static AuthenticationTicket CreateAuthenticationTicket(string apiKey)
        {
            var identity = new ClaimsIdentity(ApiKeyDefaults.Scheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, "apikey-user"));
            // Add API key prefix as claim for potential audit/tracking purposes
            identity.AddClaim(new Claim("api_key_prefix", GetApiKeyPrefix(apiKey)));
            
            var principal = new ClaimsPrincipal(identity);
            return new AuthenticationTicket(principal, ApiKeyDefaults.Scheme);
        }

        /// <summary>
        /// Extracts a safe prefix from the API key for logging purposes.
        /// This allows for debugging and audit trails without exposing the full sensitive key.
        /// Uses only the first 8 characters to balance security with utility.
        /// </summary>
        /// <param name="apiKey">The full API key</param>
        /// <returns>Safe prefix of the API key for logging</returns>
        private static string GetApiKeyPrefix(string apiKey) => 
            apiKey.Length > API_KEY_PREFIX_LENGTH ? apiKey[..API_KEY_PREFIX_LENGTH] : apiKey;

        /// <summary>
        /// Gets the remote IP address from the current HTTP request for logging purposes.
        /// Used for security monitoring and audit trails.
        /// </summary>
        /// <returns>String representation of the remote IP address, or null if unavailable</returns>
        private string? GetRemoteIpAddress() => 
            Request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
