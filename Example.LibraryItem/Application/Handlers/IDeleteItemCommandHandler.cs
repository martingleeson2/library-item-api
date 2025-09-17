namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles deletion of library items from the system.
    /// This command handler validates that the item can be safely deleted
    /// (e.g., not currently checked out), removes it from the database,
    /// and returns a success indicator.
    /// </summary>
    public interface IDeleteItemCommandHandler
    {
        /// <summary>
        /// Deletes a library item from the system.
        /// </summary>
        /// <param name="id">The unique identifier of the item to delete</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
        /// <returns>True if the item was found and deleted successfully, false if the item was not found</returns>
        Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken = default);
    }
}