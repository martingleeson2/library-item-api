using Example.LibraryItem.Application;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles retrieving a single library item by its unique identifier.
    /// This query handler returns the complete item details including location,
    /// status, metadata, and HATEOAS links if the item exists.
    /// </summary>
    public interface IGetItemQueryHandler
    {
        /// <summary>
        /// Gets a library item by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the item to retrieve</param>
        /// <param name="basePath">The base path for generating HATEOAS links</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The item if found, otherwise null</returns>
        Task<ItemDto?> HandleAsync(Guid id, string basePath, CancellationToken cancellationToken = default);
    }
}