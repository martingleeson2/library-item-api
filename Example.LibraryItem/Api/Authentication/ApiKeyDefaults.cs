using Microsoft.AspNetCore.Authentication;

namespace Example.LibraryItem.Api.Authentication
{
    public static class ApiKeyDefaults
    {
        public const string Scheme = "ApiKey";
        public const string HeaderName = "X-API-Key";
    }
}
