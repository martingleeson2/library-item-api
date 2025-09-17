using Example.LibraryItem.Application;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles full updates (PUT operations) of existing library items.
    /// This command handler performs complete replacement of an item's data,
    /// updates audit fields, and validates business rules before persisting changes.
    /// Returns the updated item or null if the item was not found.
    /// </summary>
    public interface IUpdateItemCommandHandler
    {
        /// <summary>
        /// Updates an existing library item with complete replacement of data.
        /// </summary>
        /// <param name="id">The unique identifier of the item to update</param>
        /// <param name="request">The item update request containing the complete new item data</param>
        /// <param name="basePath">The base path for generating HATEOAS links</param>
        /// <param name="user">The user updating the item (for audit trail)</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The updated item if found and updated successfully, otherwise null</returns>
        Task<ItemDto?> HandleAsync(Guid id, ItemUpdateRequestDto request, string basePath, string? user, CancellationToken cancellationToken = default);
    }
}