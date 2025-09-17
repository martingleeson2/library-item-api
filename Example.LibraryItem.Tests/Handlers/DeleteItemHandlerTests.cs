// moved from in-repo tests
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class DeleteItemHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Deleting_CheckedOut_Item_Throws_Conflict()
    {
        using var db = CreateDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item { Id = id, Title = "X", ItemType = ItemType.book, CallNumber = "1", ClassificationSystem = ClassificationSystem.dewey_decimal, Location = new ItemLocation(1, "A", "B"), Status = ItemStatus.checked_out, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var handler = new DeleteItemHandler(db, NullLogger<DeleteItemHandler>.Instance);
        var ex = await Should.ThrowAsync<InvalidOperationException>(async () => await handler.HandleAsync(id));
        ex.Message.ShouldBe("CANNOT_DELETE_CHECKED_OUT");
    }
}
