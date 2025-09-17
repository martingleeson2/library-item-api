using Microsoft.AspNetCore.Authentication;

namespace Example.LibraryItem.Api.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public List<string> ValidApiKeys { get; set; } = new();
    }
}
