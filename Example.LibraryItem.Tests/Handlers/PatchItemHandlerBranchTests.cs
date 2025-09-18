using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Example.LibraryItem.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class PatchItemHandlerBranchTests
{
    private PatchItemHandler CreateHandler(LibraryDbContext db)
    {
        var logger = Mock.Of<ILogger<PatchItemHandler>>();
        return new PatchItemHandler(
            db,
            TestHelpers.CreateValidationService(db),
            TestHelpers.CreateTestDateTimeProvider(),
            TestHelpers.CreateTestUserContext(),
            logger);
    }

    [Test]
    public async Task Returns_Null_When_Item_Not_Found()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var result = await handler.HandleAsync(Guid.NewGuid(), new ItemPatchRequestDto { Title = "X" }, "", null);
        result.ShouldBeNull();
    }

    [Test]
    public async Task Patches_Title_And_Status_Only()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item
        {
            Id = id,
            Title = "Old",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var dto = new ItemPatchRequestDto { Title = "New", Status = ItemStatus.checked_out };
        var result = await handler.HandleAsync(id, dto, "", null);

        result.ShouldNotBeNull();
        result!.Title.ShouldBe("New");
        result.Status.ShouldBe(ItemStatus.checked_out);
    }

    [Test]
    public async Task Patches_Location_When_Provided()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item
        {
            Id = id,
            Title = "T",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var dto = new ItemPatchRequestDto { 
            Location = new ItemLocationDto { Floor = 2, Section = "S", ShelfCode = "C" },
            Subtitle = "Sub",
            Author = "Auth",
            Contributors = new List<string> { "C1", "C2" },
            Issn = "1234-567X",
            Publisher = "Pub",
            PublicationDate = new DateOnly(2022,1,1),
            Edition = "1st",
            Pages = 100,
            Language = "en",
            ItemType = ItemType.dvd,
            CallNumber = "CN-2",
            ClassificationSystem = ClassificationSystem.library_of_congress,
            Collection = "Gen",
            Barcode = "B1",
            AcquisitionDate = new DateOnly(2023,1,1),
            Cost = 10m,
            ConditionNotes = "Ok",
            Description = "Desc",
            Subjects = new List<string> { "S1" },
            DigitalUrl = new Uri("https://example.org")
        };
        var result = await handler.HandleAsync(id, dto, "", null);

        result.ShouldNotBeNull();
        result!.Location.Floor.ShouldBe(2);
        result.Location.Section.ShouldBe("S");
        result.Location.ShelfCode.ShouldBe("C");
        result.ItemType.ShouldBe(ItemType.dvd);
        result.CallNumber.ShouldBe("CN-2");
        result.ClassificationSystem.ShouldBe(ClassificationSystem.library_of_congress);
        result.Subtitle.ShouldBe("Sub");
        result.Author.ShouldBe("Auth");
        result.Contributors.ShouldBe(new List<string> { "C1", "C2" });
    }

    [Test]
    public async Task Skips_Isbn_Validation_When_Null_Or_Empty()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item
        {
            Id = id,
            Title = "Test Item",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        
        // Test with null ISBN
        var dto1 = new ItemPatchRequestDto { Title = "Updated", Isbn = null };
        var result1 = await handler.HandleAsync(id, dto1, "", null);
        result1.ShouldNotBeNull();
        
        // Test with empty ISBN
        var dto2 = new ItemPatchRequestDto { Title = "Updated Again", Isbn = "" };
        var result2 = await handler.HandleAsync(id, dto2, "", null);
        result2.ShouldNotBeNull();
        result2!.Title.ShouldBe("Updated Again");
    }

    [Test]
    public async Task Validates_Isbn_When_Provided()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item
        {
            Id = id,
            Title = "Test Item",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var dto = new ItemPatchRequestDto { Isbn = "9780743273565" };
        var result = await handler.HandleAsync(id, dto, "", null);
        
        result.ShouldNotBeNull();
        result!.Isbn.ShouldBe("9780743273565");
    }

    [Test]
    public async Task Uses_Provided_User_When_Specified()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var id = Guid.NewGuid();
        db.Items.Add(new Item
        {
            Id = id,
            Title = "Test Item",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "original-user"
        });
        await db.SaveChangesAsync();

        var handler = CreateHandler(db);
        var dto = new ItemPatchRequestDto { Title = "Updated Title" };
        var result = await handler.HandleAsync(id, dto, "", "explicit-user");
        
        result.ShouldNotBeNull();
        
        // Check that the entity was updated with the explicit user
        var entity = await db.Items.FindAsync(id);
        entity.ShouldNotBeNull();
        entity!.UpdatedBy.ShouldBe("explicit-user");
    }
}
