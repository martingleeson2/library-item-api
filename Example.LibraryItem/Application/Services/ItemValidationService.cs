using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Services
{
    /// <summary>
    /// Provides business rule validation services for library items.
    /// Ensures data integrity constraints that go beyond basic field validation,
    /// such as uniqueness checks and cross-entity business rules.
    /// </summary>
    /// <param name="db">Database context for validation queries</param>
    /// <param name="logger">Logger for tracking validation operations and failures</param>
    public class ItemValidationService(LibraryDbContext db, ILogger<ItemValidationService> logger) : IItemValidationService
    {
        /// <summary>
        /// Validates that an ISBN is unique within the library system.
        /// Prevents duplicate items and maintains data integrity for book identification.
        /// Supports update scenarios by excluding a specific item ID from the uniqueness check.
        /// </summary>
        /// <param name="isbn">The ISBN to validate for uniqueness</param>
        /// <param name="excludeId">Optional item ID to exclude from validation (for updates)</param>
        /// <param name="ct">Cancellation token for request cancellation support</param>
        /// <exception cref="InvalidOperationException">Thrown when ISBN already exists with appropriate error code</exception>
        public async Task ValidateUniqueIsbnAsync(string isbn, Guid? excludeId = null, CancellationToken ct = default)
        {
            // Skip validation for null/empty ISBN - this is handled by field validation
            if (string.IsNullOrEmpty(isbn))
                return;

            // Build query to check for existing ISBN, using AsNoTracking for read-only validation
            var query = db.Items.AsNoTracking().Where(i => i.Isbn == isbn);
            
            // Exclude the specified item ID if provided (used during update operations)
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(ct);
            
            if (exists)
            {
                logger.LogWarning("ISBN validation failed: ISBN {Isbn} already exists (excluding ID: {ExcludeId})", isbn, excludeId);
                
                // Use different error codes based on operation context for proper error handling
                var errorCode = excludeId.HasValue ? "ISBN_ALREADY_EXISTS" : "ITEM_ALREADY_EXISTS";
                throw new InvalidOperationException(errorCode);
            }
            
            logger.LogDebug("ISBN validation passed for {Isbn}", isbn);
        }
    }
}