// moved from in-repo tests
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Example.LibraryItem.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class CreateItemHandlerTests
{
    private CreateItemHandler CreateHandler(LibraryDbContext db)
    {
        return new CreateItemHandler(
            db,
            TestHelpers.CreateValidationService(db),
            TestHelpers.CreateTestDateTimeProvider(),
            TestHelpers.CreateTestUserContext(),
            NullLogger<CreateItemHandler>.Instance);
    }

    [Test]
    public async Task Creates_Item_Successfully()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 F553g",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" }
        };
        var result = await handler.HandleAsync(dto, "http://localhost", "tester");
    result.Id.ShouldNotBe(Guid.Empty);
    result.Title.ShouldBe("The Great Gatsby");
        (await db.Items.CountAsync()).ShouldBe(1);
    }

    [Test]
    public async Task Duplicate_Isbn_Throws()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        db.Items.Add(new Item { Id = Guid.NewGuid(), Title = "X", ItemType = ItemType.book, CallNumber = "1", ClassificationSystem = ClassificationSystem.dewey_decimal, Location = new ItemLocation(1, "A", "B"), Isbn = "9780743273565", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 F553g",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" },
            Isbn = "9780743273565"
        };
        await Should.ThrowAsync<InvalidOperationException>(async () => await handler.HandleAsync(dto, "http://localhost", "tester"));
    }

    [Test]
    public async Task SkipsIsbnValidation_WhenIsbnIsNull()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 F553g",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" },
            Isbn = null // No ISBN validation should occur
        };
        var result = await handler.HandleAsync(dto, "http://localhost", "tester");
        result.ShouldNotBeNull();
        result.Isbn.ShouldBeNull();
    }

    [Test]
    public async Task SkipsIsbnValidation_WhenIsbnIsEmpty()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 F553g",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" },
            Isbn = "" // Empty ISBN validation should be skipped
        };
        var result = await handler.HandleAsync(dto, "http://localhost", "tester");
        result.ShouldNotBeNull();
        result.Isbn.ShouldBe("");
    }

    [Test]
    public async Task UsesProvidedUser_WhenUserParameterIsNotNull()
    {
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "The Great Gatsby",
            ItemType = ItemType.book,
            CallNumber = "813.52 F553g",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" }
        };
        var result = await handler.HandleAsync(dto, "http://localhost", "explicit-user");
        result.ShouldNotBeNull();
        
        // Verify the item was created with the explicit user
        var createdItem = await db.Items.FirstAsync();
        createdItem.CreatedBy.ShouldBe("explicit-user");
        createdItem.UpdatedBy.ShouldBe("explicit-user");
    }

    [Test]
    public async Task FallsBackToUserContext_WhenUserParameterIsNull()
    {
        // Test the branch: var currentUser = user ?? userContext.CurrentUser;
        using var db = TestHelpers.CreateInMemoryDb();
        var handler = CreateHandler(db);
        var dto = new ItemCreateRequestDto
        {
            Title = "Test Fallback",
            ItemType = ItemType.book,
            CallNumber = "TEST123",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "TEST", ShelfCode = "T-001" }
        };
        
        // Pass null as user parameter to trigger fallback to userContext.CurrentUser
        var result = await handler.HandleAsync(dto, "http://localhost", user: null);
        result.ShouldNotBeNull();
        
        // Verify the item was created with the fallback user from context
        var createdItem = await db.Items.FirstAsync();
        createdItem.CreatedBy.ShouldBe("test-user"); // From mock user context
        createdItem.UpdatedBy.ShouldBe("test-user");
    }
}
