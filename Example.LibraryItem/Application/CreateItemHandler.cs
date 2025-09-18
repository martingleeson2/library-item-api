using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application
{
    /// <summary>
    /// Handles the creation of new library items with proper validation,
    /// audit tracking, and database persistence. Implements the Command pattern
    /// as part of the CQRS architecture.
    /// </summary>
    /// <param name="db">Database context for item persistence</param>
    /// <param name="validationService">Service for business rule validation (e.g., unique ISBN)</param>
    /// <param name="dateTimeProvider">Provider for consistent datetime handling across the application</param>
    /// <param name="userContext">Context for retrieving current user information for audit trails</param>
    /// <param name="logger">Logger for tracking item creation operations and debugging</param>
    public class CreateItemHandler(
        LibraryDbContext db, 
        IItemValidationService validationService, 
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext,
        ILogger<CreateItemHandler> logger) : ICreateItemCommandHandler
    {
        /// <summary>
        /// Processes a library item creation request with full validation and audit tracking.
        /// Validates business rules (e.g., ISBN uniqueness), creates the entity with audit fields,
        /// persists to database, and returns the created item as a DTO.
        /// </summary>
        /// <param name="request">The item creation request containing all item details</param>
        /// <param name="basePath">Base URL path for constructing item links in the response</param>
        /// <param name="user">Optional user identifier for audit tracking (uses context if null)</param>
        /// <param name="ct">Cancellation token for request cancellation support</param>
        /// <returns>Created item as DTO with generated ID and audit information</returns>
        /// <exception cref="InvalidOperationException">Thrown when ISBN already exists in the system</exception>
        public async Task<ItemDto> HandleAsync(ItemCreateRequestDto request, string basePath, string? user, CancellationToken ct = default)
        {
            // Validate ISBN uniqueness if provided - prevents duplicate items in the library system
            if (!string.IsNullOrEmpty(request.Isbn))
            {
                await validationService.ValidateUniqueIsbnAsync(request.Isbn, excludeId: null, ct);
            }

            // Use injected user context if user parameter is null - supports both direct and context-based user tracking
            var currentUser = user ?? userContext.CurrentUser;
            var now = dateTimeProvider.UtcNow;
            
            // Convert DTO to entity with audit fields populated
            var entity = request.ToEntity(now, currentUser);
            // Log and persist the new item with proper audit trail
            logger.LogInformation("Creating item {ItemId} with title {Title}", entity.Id, entity.Title);
            db.Items.Add(entity);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Created item {ItemId}", entity.Id);
            
            // Convert persisted entity back to DTO for API response
            return entity.ToDto(basePath);
        }
    }
}
