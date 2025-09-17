namespace Example.LibraryItem.Application.Interfaces
{
    public interface IUserContext
    {
        /// <summary>
        /// Gets the current user identifier
        /// </summary>
        string? CurrentUser { get; }
    }
}