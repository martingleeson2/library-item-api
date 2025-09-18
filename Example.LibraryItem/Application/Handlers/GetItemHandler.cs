using Example.LibraryItem.Application;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    /// <summary>
    /// Handles retrieval of individual library items by their unique identifier.
    /// Implements the Query pattern as part of the CQRS architecture for read operations.
    /// Uses read-optimized database queries with no-tracking for better performance.
    /// </summary>
    /// <param name="db">Database context for querying library items</param>
    /// <param name="logger">Logger for tracking item retrieval operations and debugging</param>
    public class GetItemHandler(LibraryDbContext db, ILogger<GetItemHandler> logger) : IGetItemQueryHandler
    {
        /// <summary>
        /// Retrieves a specific library item by its unique identifier.
        /// Uses AsNoTracking for read-only operations to improve performance.
        /// Returns null if the item is not found, allowing the caller to handle 404 responses.
        /// </summary>
        /// <param name="id">The unique identifier of the library item to retrieve</param>
        /// <param name="basePath">Base URL path for constructing item links in the response</param>
        /// <param name="ct">Cancellation token for request cancellation support</param>
        /// <returns>Item DTO if found, null if not found</returns>
        public async Task<ItemDto?> HandleAsync(Guid id, string basePath, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching item {ItemId}", id);
            
            // Use AsNoTracking for read-only queries to improve performance and reduce memory usage
            var entity = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
            // Handle not found case with appropriate logging
            if (entity is null)
            {
                logger.LogInformation("Item {ItemId} not found", id);
                return null;
            }
            
            logger.LogInformation("Fetched item {ItemId}", id);
            
            // Convert entity to DTO with proper base path for link generation
            return entity.ToDto(basePath);
        }
    }
}
