using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class UpdateItemHandler(
        LibraryDbContext db, 
        IItemValidationService validationService, 
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext,
        ILogger<UpdateItemHandler> logger) : IUpdateItemCommandHandler
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

            if (!string.IsNullOrEmpty(request.Isbn))
            {
                await validationService.ValidateUniqueIsbnAsync(request.Isbn, excludeId: id, ct);
            }

            // Use injected user context if user parameter is null
            var currentUser = user ?? userContext.CurrentUser;
            entity.ApplyUpdate(request, dateTimeProvider.UtcNow, currentUser);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Updated item {ItemId}", id);
            return entity.ToDto(basePath);
        }
    }
}
