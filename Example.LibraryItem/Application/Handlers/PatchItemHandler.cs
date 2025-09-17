using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application.Handlers
{
    public class PatchItemHandler(
        LibraryDbContext db, 
        IItemValidationService validationService, 
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext,
        ILogger<PatchItemHandler> logger) : IPatchItemCommandHandler
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

            if (!string.IsNullOrEmpty(request.Isbn))
            {
                await validationService.ValidateUniqueIsbnAsync(request.Isbn, excludeId: id, ct);
            }

            // Use injected user context if user parameter is null
            var currentUser = user ?? userContext.CurrentUser;
            entity.ApplyPatch(request, dateTimeProvider.UtcNow, currentUser);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Patched item {ItemId}", id);
            return entity.ToDto(basePath);
        }
    }
}
