using Example.LibraryItem.Application;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles partial updates (PATCH operations) of existing library items.
    /// This command handler applies selective updates to specific fields,
    /// leaving other fields unchanged. Only non-null fields in the request
    /// are updated. Returns the updated item or null if not found.
    /// </summary>
    public interface IPatchItemCommandHandler
    {
        /// <summary>
        /// Partially updates an existing library item.
        /// </summary>
        /// <param name="id">The unique identifier of the item to patch</param>
        /// <param name="request">The item patch request containing only fields to update (null fields are ignored)</param>
        /// <param name="basePath">The base path for generating HATEOAS links</param>
        /// <param name="user">The user patching the item (for audit trail)</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>The updated item if found and patched successfully, otherwise null</returns>
        Task<ItemDto?> HandleAsync(Guid id, ItemPatchRequestDto request, string basePath, string? user, CancellationToken cancellationToken = default);
    }
}