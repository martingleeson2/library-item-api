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
            Page: 1, Limit: 3,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
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
            Page: 1, Limit: 10,
            Title: "Great", Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: ItemStatus.checked_out, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.ShouldNotBeNull();
    result.Data.Count.ShouldBe(0);
    result.Pagination.TotalItems.ShouldBe(0);
    result.Pagination.TotalPages.ShouldBe(0);
    }

    [Test]
    public async Task Filters_By_ItemType()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Book Item",
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
                Title = "DVD Item",
                ItemType = ItemType.dvd,
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: ItemType.dvd, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].ItemType.ShouldBe(ItemType.dvd);
    }

    [Test]
    public async Task Filters_By_CheckedOut_Status()
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: ItemStatus.checked_out, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].Status.ShouldBe(ItemStatus.checked_out);
    }

    [Test]
    public async Task Filters_By_LocationFloor()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Floor 1 Book",
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
                Title = "Floor 2 Book",
                ItemType = ItemType.book,
                CallNumber = "002.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(2, "A", "B"),
                Status = ItemStatus.available,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );
        await db.SaveChangesAsync();

        var handler = new ListItemsHandler(db, logger);
        var query = new ListItemsQuery(
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: 2, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].Location.Floor.ShouldBe(2);
    }

    [Test]
    public async Task Sorts_By_Author_Ascending()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Book by Zebra",
                Author = "Zebra Author",
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
                Title = "Book by Alpha",
                Author = "Alpha Author",
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: "author", SortOrder: "asc"
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(2);
        result.Data[0].Author.ShouldBe("Alpha Author");
        result.Data[1].Author.ShouldBe("Zebra Author");
    }

    [Test]
    public async Task Filters_By_PublicationYearRange()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Old Book",
                PublicationDate = new DateOnly(1990, 1, 1),
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
                Title = "New Book",
                PublicationDate = new DateOnly(2020, 1, 1),
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
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: 2000, PublicationYearTo: 2025,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].Title.ShouldBe("New Book");
    }

    [Test]
    public async Task Filters_With_Null_Author_Comparison()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        
        db.Items.AddRange(
            new Item
            {
                Id = Guid.NewGuid(),
                Title = "Book with Author",
                Author = "Test Author",
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
                Title = "Book without Author",
                Author = null,
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
            Page: 1, Limit: 10,
            Title: null, Author: "Test", Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: null, SortOrder: null
        );

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.Data.Count.ShouldBe(1);
        result.Data[0].Author.ShouldBe("Test Author");
    }
}