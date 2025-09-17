using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Example.LibraryItem.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class UpdateItemHandlerTests
{
    private UpdateItemHandler CreateHandler(LibraryDbContext db)
    {
        var logger = Mock.Of<ILogger<UpdateItemHandler>>();
        return new UpdateItemHandler(
            db,
            TestHelpers.CreateValidationService(db),
            TestHelpers.CreateTestDateTimeProvider(),
            TestHelpers.CreateTestUserContext(),
            logger);
    }

    [Test]
    public async Task Updates_Item_Successfully()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDb();
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

        var handler = CreateHandler(db);
        var updateRequest = new ItemUpdateRequestDto
        {
            Title = "Updated Title",
            ItemType = ItemType.book,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 2, Section = "B", ShelfCode = "C-126" },
            Status = ItemStatus.checked_out
        };

        // Act
        var result = await handler.HandleAsync(itemId, updateRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
    result.Title.ShouldBe("Updated Title");
    result.CallNumber.ShouldBe("002.42");
    result.Status.ShouldBe(ItemStatus.checked_out);
    result.Location.Floor.ShouldBe(2);
    result.Location.Section.ShouldBe("B");
    result.Location.ShelfCode.ShouldBe("C-126");
    }

    [Test]
    public async Task Returns_Null_When_Item_Not_Found()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var updateRequest = new ItemUpdateRequestDto
        {
            Title = "Updated Title",
            ItemType = ItemType.book,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 2, Section = "B", ShelfCode = "C-126" },
            Status = ItemStatus.available
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
        using var db = TestHelpers.CreateInMemoryDb();
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
            Isbn = "9780321570512",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        db.Items.AddRange(item1, item2);
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var updateRequest = new ItemUpdateRequestDto
        {
            Title = "Updated Item 2",
            ItemType = ItemType.book,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            Status = ItemStatus.available,
            Isbn = "9780743273565" // Using item1's ISBN
        };

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => 
            await handler.HandleAsync(itemId2, updateRequest, "http://localhost", "test-user"));
    }

    [Test]
    public async Task Allows_Same_Isbn_For_Same_Item()
    {
        // Arrange
        using var db = TestHelpers.CreateInMemoryDb();
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

        var handler = CreateHandler(db);
        var updateRequest = new ItemUpdateRequestDto
        {
            Title = "Updated Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            Status = ItemStatus.available,
            Isbn = "9780743273565" // Same ISBN
        };

        // Act
        var result = await handler.HandleAsync(itemId, updateRequest, "http://localhost", "test-user");

        // Assert
        result.ShouldNotBeNull();
        result.Title.ShouldBe("Updated Title");
        result.Isbn.ShouldBe("9780743273565");
    }
}