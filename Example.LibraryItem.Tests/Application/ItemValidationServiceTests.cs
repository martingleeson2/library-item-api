using Example.LibraryItem.Application.Services;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Example.LibraryItem.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Application;

public class ItemValidationServiceTests
{
    [Test]
    public async Task ValidateUniqueIsbnAsync_ReturnsEarly_WhenIsbnIsNull()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);

        // Should not throw - null ISBN should return early
        await service.ValidateUniqueIsbnAsync(null!);
    }

    [Test]
    public async Task ValidateUniqueIsbnAsync_ReturnsEarly_WhenIsbnIsEmpty()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);

        // Should not throw - empty ISBN should return early
        await service.ValidateUniqueIsbnAsync("");
        await service.ValidateUniqueIsbnAsync("   ");
    }

    [Test]
    public async Task ValidateUniqueIsbnAsync_ThrowsItemAlreadyExists_WhenIsbnExistsAndNoExcludeId()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        
        // Add an item with ISBN
        db.Items.Add(new Item 
        { 
            Id = Guid.NewGuid(), 
            Title = "Test Book", 
            ItemType = ItemType.book, 
            CallNumber = "123", 
            ClassificationSystem = ClassificationSystem.dewey_decimal, 
            Location = new ItemLocation(1, "A", "B"), 
            Isbn = "9780743273565", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        });
        await db.SaveChangesAsync();

        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);
        
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await service.ValidateUniqueIsbnAsync("9780743273565"));
        
        exception.Message.ShouldBe("ITEM_ALREADY_EXISTS");
    }

    [Test]
    public async Task ValidateUniqueIsbnAsync_ThrowsIsbnAlreadyExists_WhenIsbnExistsAndHasExcludeId()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        
        var existingId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        
        // Add an item with ISBN
        db.Items.Add(new Item 
        { 
            Id = existingId, 
            Title = "Test Book", 
            ItemType = ItemType.book, 
            CallNumber = "123", 
            ClassificationSystem = ClassificationSystem.dewey_decimal, 
            Location = new ItemLocation(1, "A", "B"), 
            Isbn = "9780743273565", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        });
        await db.SaveChangesAsync();

        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);
        
        // When excluding a different ID, should throw ISBN_ALREADY_EXISTS
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await service.ValidateUniqueIsbnAsync("9780743273565", differentId));
        
        exception.Message.ShouldBe("ISBN_ALREADY_EXISTS");
    }

    [Test]
    public async Task ValidateUniqueIsbnAsync_DoesNotThrow_WhenIsbnExistsButExcludedIdMatches()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        
        var existingId = Guid.NewGuid();
        
        // Add an item with ISBN
        db.Items.Add(new Item 
        { 
            Id = existingId, 
            Title = "Test Book", 
            ItemType = ItemType.book, 
            CallNumber = "123", 
            ClassificationSystem = ClassificationSystem.dewey_decimal, 
            Location = new ItemLocation(1, "A", "B"), 
            Isbn = "9780743273565", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        });
        await db.SaveChangesAsync();

        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);
        
        // When excluding the same ID, should not throw (updating same item)
        await service.ValidateUniqueIsbnAsync("9780743273565", existingId);
    }

    [Test]
    public async Task ValidateUniqueIsbnAsync_DoesNotThrow_WhenIsbnIsUnique()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        
        // Add an item with different ISBN
        db.Items.Add(new Item 
        { 
            Id = Guid.NewGuid(), 
            Title = "Test Book", 
            ItemType = ItemType.book, 
            CallNumber = "123", 
            ClassificationSystem = ClassificationSystem.dewey_decimal, 
            Location = new ItemLocation(1, "A", "B"), 
            Isbn = "9780743273565", 
            CreatedAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        });
        await db.SaveChangesAsync();

        var service = new ItemValidationService(db, NullLogger<ItemValidationService>.Instance);
        
        // Should not throw - different ISBN
        await service.ValidateUniqueIsbnAsync("9781234567890");
    }
}