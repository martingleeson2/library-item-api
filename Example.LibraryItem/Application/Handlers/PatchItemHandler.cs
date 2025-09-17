using Example.LibraryItem.Application;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class PatchItemHandler(LibraryDbContext db, ILogger<PatchItemHandler> logger) : IPatchItemCommandHandler
    {
        public async Task<ItemDto?> HandleAsync(Guid id, ItemPatchRequestDto request, string basePath, string? user, CancellationToken ct = default)
        {
            logger.LogInformation("Patching item {ItemId}", id);
            var entity = await db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (entity is null)
            {
                logger.LogInformation("Patch: item {ItemId} not found", id);
                return null;
            }

            if (!string.IsNullOrEmpty(request.isbn))
            {
                var exists = await db.Items.AsNoTracking().AnyAsync(i => i.Id != id && i.Isbn == request.isbn, ct);
                if (exists)
                {
                    logger.LogWarning("Patch conflict for item {ItemId} - duplicate ISBN {Isbn}", id, request.isbn);
                    throw new InvalidOperationException("ISBN_ALREADY_EXISTS");
                }
            }

            entity.ApplyPatch(request, DateTime.UtcNow, user);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Patched item {ItemId}", id);
            return entity.ToDto(basePath);
        }
    }
}
