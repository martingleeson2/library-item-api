using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class PatchItemHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Patches_Item_Successfully()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        var itemId = Guid.NewGuid();
        var originalItem = new Item
        {
            Id = itemId,
            Title = "Original Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Items.Add(originalItem);
        await db.SaveChangesAsync();

        var handler = new PatchItemHandler(db, logger);
        var patchRequest = new ItemPatchRequestDto
        {
            title = "Updated Title",
            status = ItemStatus.checked_out
            // Only patching title and status, leaving other fields unchanged
        };

        // Act
        var result = await handler.HandleAsync(itemId, patchRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.title.ShouldBe("Updated Title");
        result.status.ShouldBe(ItemStatus.checked_out);
        result.call_number.ShouldBe("001.42"); // Should remain unchanged
        result.item_type.ShouldBe(ItemType.book); // Should remain unchanged
    }

    [Test]
    public async Task Returns_Null_When_Item_Not_Found()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        var handler = new PatchItemHandler(db, logger);
        var patchRequest = new ItemPatchRequestDto
        {
            title = "Updated Title"
        };

        // Act
        var result = await handler.HandleAsync(Guid.NewGuid(), patchRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Throws_When_Duplicate_Isbn()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();
        
        var item1 = new Item
        {
            Id = itemId1,
            Title = "Item 1",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            Isbn = "9780743273565",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var item2 = new Item
        {
            Id = itemId2,
            Title = "Item 2",
            ItemType = ItemType.book,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Items.AddRange(item1, item2);
        await db.SaveChangesAsync();

        var handler = new PatchItemHandler(db, logger);
        var patchRequest = new ItemPatchRequestDto
        {
            isbn = "9780743273565" // Same as item1
        };

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(itemId2, patchRequest, "http://localhost", "test-user"));
        ex.Message.ShouldBe("ISBN_ALREADY_EXISTS");
    }

    [Test]
    public async Task Patches_Location_Only()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        var itemId = Guid.NewGuid();
        var originalItem = new Item
        {
            Id = itemId,
            Title = "Original Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Items.Add(originalItem);
        await db.SaveChangesAsync();

        var handler = new PatchItemHandler(db, logger);
        var patchRequest = new ItemPatchRequestDto
        {
            location = new ItemLocationDto { floor = 2, section = "C", shelf_code = "D-127" }
        };

        // Act
        var result = await handler.HandleAsync(itemId, patchRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.location.floor.ShouldBe(2);
        result.location.section.ShouldBe("C");
        result.location.shelf_code.ShouldBe("D-127");
        result.title.ShouldBe("Original Title"); // Should remain unchanged
    }

    [Test]
    public async Task Patches_With_Null_Fields_Leaves_Original_Values()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        var itemId = Guid.NewGuid();
        var originalItem = new Item
        {
            Id = itemId,
            Title = "Original Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            Isbn = "9780743273565",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Items.Add(originalItem);
        await db.SaveChangesAsync();

        var handler = new PatchItemHandler(db, logger);
        var patchRequest = new ItemPatchRequestDto
        {
            title = null, // Should not change original title
            isbn = null,  // Should not change original ISBN
            status = ItemStatus.checked_out // Should change status
        };

        // Act
        var result = await handler.HandleAsync(itemId, patchRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.title.ShouldBe("Original Title"); // Should remain unchanged
        result.isbn.ShouldBe("9780743273565"); // Should remain unchanged
        result.status.ShouldBe(ItemStatus.checked_out); // Should be changed
    }
}