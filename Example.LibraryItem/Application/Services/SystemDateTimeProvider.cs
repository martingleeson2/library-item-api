using Example.LibraryItem.Application.Interfaces;

namespace Example.LibraryItem.Application.Services
{
    public class SystemDateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}