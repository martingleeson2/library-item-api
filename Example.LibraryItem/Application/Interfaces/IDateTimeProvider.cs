namespace Example.LibraryItem.Application.Interfaces
{
    public interface IDateTimeProvider
    {
        /// <summary>
        /// Gets the current UTC date and time
        /// </summary>
        DateTime UtcNow { get; }
    }
}