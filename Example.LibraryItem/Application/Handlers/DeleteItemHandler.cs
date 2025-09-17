using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class DeleteItemHandler(LibraryDbContext db, ILogger<DeleteItemHandler> logger) : IDeleteItemCommandHandler
    {
        public async Task<bool> HandleAsync(Guid id, CancellationToken ct = default)
        {
            logger.LogInformation("Deleting item {ItemId}", id);
            var entity = await db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
            if (entity is null)
            {
                logger.LogInformation("Delete: item {ItemId} not found", id);
                return false;
            }

            if (entity.Status == ItemStatus.checked_out)
            {
                logger.LogWarning("Delete conflict for item {ItemId} - checked out", id);
                throw new InvalidOperationException("CANNOT_DELETE_CHECKED_OUT");
            }

            db.Items.Remove(entity);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Deleted item {ItemId}", id);
            return true;
        }
    }
}
