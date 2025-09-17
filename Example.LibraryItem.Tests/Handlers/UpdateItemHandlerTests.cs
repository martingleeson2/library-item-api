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

public class UpdateItemHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Updates_Item_Successfully()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<UpdateItemHandler>>();
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

        var handler = new UpdateItemHandler(db, logger);
        var updateRequest = new ItemUpdateRequestDto
        {
            title = "Updated Title",
            item_type = ItemType.book,
            call_number = "002.42",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 2, section = "B", shelf_code = "C-126" },
            status = ItemStatus.checked_out
        };

        // Act
        var result = await handler.HandleAsync(itemId, updateRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.title.ShouldBe("Updated Title");
        result.call_number.ShouldBe("002.42");
        result.status.ShouldBe(ItemStatus.checked_out);
        result.location.floor.ShouldBe(2);
        result.location.section.ShouldBe("B");
        result.location.shelf_code.ShouldBe("C-126");
    }

    [Test]
    public async Task Returns_Null_When_Item_Not_Found()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<UpdateItemHandler>>();
        var handler = new UpdateItemHandler(db, logger);
        var updateRequest = new ItemUpdateRequestDto
        {
            title = "Updated Title",
            item_type = ItemType.book,
            call_number = "002.42",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 2, section = "B", shelf_code = "C-126" },
            status = ItemStatus.available
        };

        // Act
        var result = await handler.HandleAsync(Guid.NewGuid(), updateRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Throws_When_Duplicate_Isbn()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<UpdateItemHandler>>();
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

        var handler = new UpdateItemHandler(db, logger);
        var updateRequest = new ItemUpdateRequestDto
        {
            title = "Updated Title",
            item_type = ItemType.book,
            call_number = "002.42",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 2, section = "B", shelf_code = "C-126" },
            status = ItemStatus.available,
            isbn = "9780743273565" // Same as item1
        };

        // Act & Assert
        var ex = await Should.ThrowAsync<InvalidOperationException>(
            async () => await handler.HandleAsync(itemId2, updateRequest, "http://localhost", "test-user"));
        ex.Message.ShouldBe("ISBN_ALREADY_EXISTS");
    }

    [Test]
    public async Task Allows_Same_Isbn_For_Same_Item()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<UpdateItemHandler>>();
        var itemId = Guid.NewGuid();
        
        var item = new Item
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
        
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var handler = new UpdateItemHandler(db, logger);
        var updateRequest = new ItemUpdateRequestDto
        {
            title = "Updated Title",
            item_type = ItemType.book,
            call_number = "002.42",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 2, section = "B", shelf_code = "C-126" },
            status = ItemStatus.available,
            isbn = "9780743273565" // Same ISBN as before
        };

        // Act
        var result = await handler.HandleAsync(itemId, updateRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.isbn.ShouldBe("9780743273565");
        result.title.ShouldBe("Updated Title");
    }
}