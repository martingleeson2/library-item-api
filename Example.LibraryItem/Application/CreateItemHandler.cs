using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application
{
    public class CreateItemHandler(LibraryDbContext db, ILogger<CreateItemHandler> logger) : ICreateItemCommandHandler
    {
        public async Task<ItemDto> HandleAsync(ItemCreateRequestDto request, string basePath, string? user, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(request.isbn))
            {
                var exists = await db.Items.AnyAsync(i => i.Isbn == request.isbn, ct);
                if (exists)
                {
                    logger.LogWarning("Create conflict for ISBN {Isbn}", request.isbn);
                    throw new InvalidOperationException("ITEM_ALREADY_EXISTS");
                }
            }

            var now = DateTime.UtcNow;
            var entity = request.ToEntity(now, user);
            logger.LogInformation("Creating item {ItemId} with title {Title}", entity.Id, entity.Title);
            db.Items.Add(entity);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Created item {ItemId}", entity.Id);
            return entity.ToDto(basePath);
        }
    }
}
