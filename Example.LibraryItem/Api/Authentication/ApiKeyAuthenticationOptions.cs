using Microsoft.AspNetCore.Authentication;

namespace Example.LibraryItem.Api.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public List<string> ValidApiKeys { get; set; } = new();
        
        /// <summary>
        /// HTTP header name to look for the API key
        /// </summary>
        public string HeaderName { get; set; } = "X-API-Key";

        /// <summary>
        /// Whether to also check query parameters for the API key
        /// </summary>
        public bool AllowQueryParameter { get; set; } = false;

        /// <summary>
        /// Query parameter name when AllowQueryParameter is true
        /// </summary>
        public string QueryParameterName { get; set; } = "api_key";
    }
}
