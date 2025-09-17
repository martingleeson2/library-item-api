namespace Example.LibraryItem.Application.Interfaces
{
    public interface IItemValidationService
    {
        /// <summary>
        /// Validates that the given ISBN is unique in the system
        /// </summary>
        /// <param name="isbn">The ISBN to validate</param>
        /// <param name="excludeId">Optional ID to exclude from the uniqueness check (for updates)</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Throws InvalidOperationException if ISBN already exists</returns>
        Task ValidateUniqueIsbnAsync(string isbn, Guid? excludeId = null, CancellationToken ct = default);
    }
}