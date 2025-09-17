// moved from in-repo tests
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class CreateItemHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Creates_Item_Successfully()
    {
        using var db = CreateDb();
        var handler = new CreateItemHandler(db, NullLogger<CreateItemHandler>.Instance);
        var dto = new ItemCreateRequestDto
        {
            title = "The Great Gatsby",
            item_type = ItemType.book,
            call_number = "813.52 F553g",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 1, section = "REF", shelf_code = "A-125" }
        };
        var result = await handler.HandleAsync(dto, "http://localhost", "tester");
        result.id.ShouldNotBe(Guid.Empty);
        result.title.ShouldBe("The Great Gatsby");
        (await db.Items.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task Duplicate_Isbn_Throws()
    {
        using var db = CreateDb();
        db.Items.Add(new Item { Id = Guid.NewGuid(), Title = "X", ItemType = ItemType.book, CallNumber = "1", ClassificationSystem = ClassificationSystem.dewey_decimal, Location = new ItemLocation(1, "A", "B"), Isbn = "9780743273565", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var handler = new CreateItemHandler(db, NullLogger<CreateItemHandler>.Instance);
        var dto = new ItemCreateRequestDto
        {
            title = "The Great Gatsby",
            item_type = ItemType.book,
            call_number = "813.52 F553g",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 1, section = "REF", shelf_code = "A-125" },
            isbn = "9780743273565"
        };
        await Should.ThrowAsync<InvalidOperationException>(async () => await handler.HandleAsync(dto, "http://localhost", "tester"));
    }
}
