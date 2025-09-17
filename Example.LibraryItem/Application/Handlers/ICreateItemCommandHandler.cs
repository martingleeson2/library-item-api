using Example.LibraryItem.Application;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles creating new library items in the system.
    /// This command handler validates the input, assigns a unique ID,
    /// sets audit fields, and persists the new item to the database.
    /// Returns the created item with generated metadata and HATEOAS links.
    /// </summary>
    public interface ICreateItemCommandHandler
    {
        /// <summary>
        /// Creates a new library item.
        /// </summary>
        /// <param name="request">The item creation request containing all required and optional item data</param>
        /// <param name="basePath">The base path for generating HATEOAS links</param>
        /// <param name="user">The user creating the item (for audit trail)</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The created item with generated ID and audit fields</returns>
        Task<ItemDto> HandleAsync(ItemCreateRequestDto request, string basePath, string? user, CancellationToken cancellationToken = default);
    }
}