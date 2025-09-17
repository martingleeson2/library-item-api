using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Services
{
    public class ItemValidationService(LibraryDbContext db, ILogger<ItemValidationService> logger) : IItemValidationService
    {
        public async Task ValidateUniqueIsbnAsync(string isbn, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(isbn))
                return;

            var query = db.Items.AsNoTracking().Where(i => i.Isbn == isbn);
            
            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(ct);
            
            if (exists)
            {
                logger.LogWarning("ISBN validation failed: ISBN {Isbn} already exists (excluding ID: {ExcludeId})", isbn, excludeId);
                
                // Use different error codes based on operation context
                var errorCode = excludeId.HasValue ? "ISBN_ALREADY_EXISTS" : "ITEM_ALREADY_EXISTS";
                throw new InvalidOperationException(errorCode);
            }
            
            logger.LogDebug("ISBN validation passed for {Isbn}", isbn);
        }
    }
}