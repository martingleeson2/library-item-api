using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Example.LibraryItem.Application
{
    public class CreateItemHandler(
        LibraryDbContext db, 
        IItemValidationService validationService, 
        IDateTimeProvider dateTimeProvider,
        IUserContext userContext,
        ILogger<CreateItemHandler> logger) : ICreateItemCommandHandler
    {
        public async Task<ItemDto> HandleAsync(ItemCreateRequestDto request, string basePath, string? user, CancellationToken ct = default)
        {
            if (!string.IsNullOrEmpty(request.Isbn))
            {
                await validationService.ValidateUniqueIsbnAsync(request.Isbn, excludeId: null, ct);
            }

            // Use injected user context if user parameter is null
            var currentUser = user ?? userContext.CurrentUser;
            var now = dateTimeProvider.UtcNow;
            var entity = request.ToEntity(now, currentUser);
            logger.LogInformation("Creating item {ItemId} with title {Title}", entity.Id, entity.Title);
            db.Items.Add(entity);
            await db.SaveChangesAsync(ct);
            logger.LogInformation("Created item {ItemId}", entity.Id);
            return entity.ToDto(basePath);
        }
    }
}
