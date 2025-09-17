using Example.LibraryItem.Application;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class GetItemHandler(LibraryDbContext db, ILogger<GetItemHandler> logger) : IGetItemQueryHandler
    {
        public async Task<ItemDto?> HandleAsync(Guid id, string basePath, CancellationToken ct = default)
        {
            logger.LogInformation("Fetching item {ItemId}", id);
            var entity = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
            if (entity is null)
            {
                logger.LogInformation("Item {ItemId} not found", id);
                return null;
            }
            logger.LogInformation("Fetched item {ItemId}", id);
            return entity?.ToDto(basePath);
        }
    }
}
