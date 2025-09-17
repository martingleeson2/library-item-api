using Example.LibraryItem.Application;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class UpdateItemHandler(LibraryDbContext db, ILogger<UpdateItemHandler> logger) : IUpdateItemCommandHandler
    {
        public async Task<ItemDto?> HandleAsync(Guid id, ItemUpdateRequestDto request, string basePath, string? user, CancellationToken ct = default)
        {
            logger.LogInformation("Updating item {ItemId}", id);
            var entity = await db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (entity is null)
            {
                logger.LogInformation("Update: item {ItemId} not found", id);
                return null;
            }

            if (!string.IsNullOrEmpty(request.isbn))
            {
                var exists = await db.Items.AsNoTracking().AnyAsync(i => i.Id != id && i.Isbn == request.isbn, ct);
                if (exists)
                {
                    logger.LogWarning("Update conflict for item {ItemId} - duplicate ISBN {Isbn}", id, request.isbn);
                    throw new InvalidOperationException("ISBN_ALREADY_EXISTS");
                }
            }

            entity.ApplyUpdate(request, DateTime.UtcNow, user);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Updated item {ItemId}", id);
            return entity.ToDto(basePath);
        }
    }
}
