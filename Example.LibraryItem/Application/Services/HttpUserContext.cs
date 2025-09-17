using Example.LibraryItem.Application.Interfaces;

namespace Example.LibraryItem.Application.Services
{
    public class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        public string? CurrentUser => httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}