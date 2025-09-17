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

public class ListItemsHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Returns_Paginated_Items()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        // Add test items
        for (int i = 1; i <= 5; i++)
        {
            db.Items.Add(new Item
            {
                Id = Guid.NewGuid(),
                Title = $"Book {i}",
                ItemType = ItemType.book,
                CallNumber = $"00{i}.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", "B"),
                Status = ItemStatus.available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var handler = new ListItemsHandler(db, logger);
        var query = new ListItemsQuery(
            page: 1, limit: 3,
            title: null, author: null, isbn: null,
            item_type: null, status: null, collection: null,
            location_floor: null, location_section: null, call_number: null,
            publication_year_from: null, publication_year_to: null,
            sort_by: null, sort_order: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(3);
    result.Pagination.TotalItems.ShouldBe(5);
    result.Pagination.Page.ShouldBe(1);
    result.Pagination.Limit.ShouldBe(3);
    result.Pagination.TotalPages.ShouldBe(2);
    }

    [Test]
    public async Task Filters_By_Title()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "The Great Gatsby",
                ItemType = ItemType.book,
                CallNumber = "001.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", "B"),
                Status = ItemStatus.available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "To Kill a Mockingbird",
                ItemType = ItemType.book,
                CallNumber = "002.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", "B"),
                Status = ItemStatus.available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var handler = new ListItemsHandler(db, logger);
        var query = new ListItemsQuery(
            page: 1, limit: 10,
            title: "Great", author: null, isbn: null,
            item_type: null, status: null, collection: null,
            location_floor: null, location_section: null, call_number: null,
            publication_year_from: null, publication_year_to: null,
            sort_by: null, sort_order: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(1);
    result.Data[0].Title.ShouldBe("The Great Gatsby");
    result.Pagination.TotalItems.ShouldBe(1);
    }

    [Test]
    public async Task Filters_By_Status()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Available Book",
                ItemType = ItemType.book,
                CallNumber = "001.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", "B"),
                Status = ItemStatus.available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Checked Out Book",
                ItemType = ItemType.book,
                CallNumber = "002.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", "B"),
                Status = ItemStatus.checked_out,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var handler = new ListItemsHandler(db, logger);
        var query = new ListItemsQuery(
            page: 1, limit: 10,
            title: null, author: null, isbn: null,
            item_type: null, status: ItemStatus.checked_out, collection: null,
            location_floor: null, location_section: null, call_number: null,
            publication_year_from: null, publication_year_to: null,
            sort_by: null, sort_order: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(1);
    result.Data[0].Title.ShouldBe("Checked Out Book");
    result.Data[0].Status.ShouldBe(ItemStatus.checked_out);
    result.Pagination.TotalItems.ShouldBe(1);
    }

    [Test]
    public async Task Returns_Empty_List_When_No_Items()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);
        var query = new ListItemsQuery(
            page: 1, limit: 10,
            title: null, author: null, isbn: null,
            item_type: null, status: null, collection: null,
            location_floor: null, location_section: null, call_number: null,
            publication_year_from: null, publication_year_to: null,
            sort_by: null, sort_order: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(0);
    result.Pagination.TotalItems.ShouldBe(0);
    result.Pagination.TotalPages.ShouldBe(0);
    }
}