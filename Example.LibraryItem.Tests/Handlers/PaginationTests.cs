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

[TestFixture]
public class PaginationTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    private async Task<LibraryDbContext> CreateDbWithItems(int itemCount)
    {
        var db = CreateDb();
        
        for (int i = 1; i <= itemCount; i++)
        {
            db.Items.Add(new Item
            {
                Id = Guid.NewGuid(),
                Title = $"Test Book {i:D3}",
                Author = $"Author {i}",
                ItemType = ItemType.book,
                CallNumber = $"{i:D3}.42",
                ClassificationSystem = ClassificationSystem.dewey_decimal,
                Location = new ItemLocation(1, "A", $"A{i}"),
                Status = i % 3 == 0 ? ItemStatus.checked_out : ItemStatus.available,
                Publisher = $"Publisher {i}",
                Cost = i * 10.50m,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        
        await db.SaveChangesAsync();
        return db;
    }

    [Test]
    public async Task Pagination_FirstPage_ReturnsCorrectItems()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
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
        result.Data.Count.ShouldBe(10);
        result.Pagination.Page.ShouldBe(1);
        result.Pagination.Limit.ShouldBe(10);
        result.Pagination.TotalItems.ShouldBe(20);
        result.Pagination.TotalPages.ShouldBe(2);
        result.Pagination.HasNext.ShouldBeTrue();
        result.Pagination.HasPrevious.ShouldBeFalse();
    }

    [Test]
    public async Task Pagination_SecondPage_ReturnsCorrectItems()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        var query = new ListItemsQuery(
            Page: 2, Limit: 10,
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
        result.Data.Count.ShouldBe(10);
        result.Pagination.Page.ShouldBe(2);
        result.Pagination.Limit.ShouldBe(10);
        result.Pagination.TotalItems.ShouldBe(20);
        result.Pagination.TotalPages.ShouldBe(2);
        result.Pagination.HasNext.ShouldBeFalse();
        result.Pagination.HasPrevious.ShouldBeTrue();
    }

    [Test]
    public async Task Pagination_SmallPageSize_ReturnsMultiplePages()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        var query = new ListItemsQuery(
            Page: 1, Limit: 5,
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
        result.Data.Count.ShouldBe(5);
        result.Pagination.Page.ShouldBe(1);
        result.Pagination.Limit.ShouldBe(5);
        result.Pagination.TotalItems.ShouldBe(20);
        result.Pagination.TotalPages.ShouldBe(4); // 20 items / 5 per page = 4 pages
        result.Pagination.HasNext.ShouldBeTrue();
        result.Pagination.HasPrevious.ShouldBeFalse();
    }

    [Test]
    public async Task Pagination_MiddlePage_HasBothNextAndPrevious()
    {
        // Arrange
        using var db = await CreateDbWithItems(50);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        var query = new ListItemsQuery(
            Page: 3, Limit: 10,
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
        result.Data.Count.ShouldBe(10);
        result.Pagination.Page.ShouldBe(3);
        result.Pagination.Limit.ShouldBe(10);
        result.Pagination.TotalItems.ShouldBe(50);
        result.Pagination.TotalPages.ShouldBe(5);
        result.Pagination.HasNext.ShouldBeTrue();
        result.Pagination.HasPrevious.ShouldBeTrue();
    }

    [Test]
    public async Task Pagination_LastPagePartial_ReturnsCorrectCount()
    {
        // Arrange
        using var db = await CreateDbWithItems(23); // 23 items with limit 10 = 3 pages (last page has 3 items)
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        var query = new ListItemsQuery(
            Page: 3, Limit: 10,
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
        result.Data.Count.ShouldBe(3); // Only 3 items on last page
        result.Pagination.Page.ShouldBe(3);
        result.Pagination.Limit.ShouldBe(10);
        result.Pagination.TotalItems.ShouldBe(23);
        result.Pagination.TotalPages.ShouldBe(3);
        result.Pagination.HasNext.ShouldBeFalse();
        result.Pagination.HasPrevious.ShouldBeTrue();
    }

    [Test]
    public async Task Pagination_PageBeyondTotal_ReturnsEmptyResult()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        var query = new ListItemsQuery(
            Page: 5, Limit: 10, // Page 5 when there are only 2 pages
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
        result.Pagination.Page.ShouldBe(5);
        result.Pagination.Limit.ShouldBe(10);
        result.Pagination.TotalItems.ShouldBe(20);
        result.Pagination.TotalPages.ShouldBe(2);
        result.Pagination.HasNext.ShouldBeFalse();
        result.Pagination.HasPrevious.ShouldBeTrue();
    }

    [Test]
    public async Task Pagination_WithFiltering_CalculatesCorrectTotals()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        // Filter for checked out items (every 3rd item)
        var query = new ListItemsQuery(
            Page: 1, Limit: 5,
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
        var expectedCheckedOutItems = 20 / 3; // Every 3rd item = 6 items (items 3, 6, 9, 12, 15, 18)
        result.Data.Count.ShouldBeLessThanOrEqualTo(5); // Limited by page size
        result.Pagination.TotalItems.ShouldBe(expectedCheckedOutItems);
        result.Data.ShouldAllBe(item => item.Status == ItemStatus.checked_out);
    }

    [Test]
    public async Task Pagination_Sorting_MaintainsConsistentOrder()
    {
        // Arrange
        using var db = await CreateDbWithItems(20);
        var logger = Mock.Of<ILogger<ListItemsHandler>>();
        var handler = new ListItemsHandler(db, logger);

        // Get first page sorted by title
        var query1 = new ListItemsQuery(
            Page: 1, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: "title", SortOrder: "asc"
        );

        var query2 = new ListItemsQuery(
            Page: 2, Limit: 10,
            Title: null, Author: null, Isbn: null,
            ItemType: null, Status: null, Collection: null,
            LocationFloor: null, LocationSection: null, CallNumber: null,
            PublicationYearFrom: null, PublicationYearTo: null,
            SortBy: "title", SortOrder: "asc"
        );

        // Act
        var page1 = await handler.HandleAsync(query1);
        var page2 = await handler.HandleAsync(query2);

        // Assert
        page1.ShouldNotBeNull();
        page2.ShouldNotBeNull();
        
        // Verify no duplicate items across pages
        var page1Ids = page1.Data.Select(x => x.Id).ToHashSet();
        var page2Ids = page2.Data.Select(x => x.Id).ToHashSet();
        page1Ids.Intersect(page2Ids).ShouldBeEmpty();
        
        // Verify sorting order is maintained
        var page1Titles = page1.Data.Select(x => x.Title).ToList();
        var page2Titles = page2.Data.Select(x => x.Title).ToList();
        page1Titles.ShouldBe(page1Titles.OrderBy(x => x).ToList());
        page2Titles.ShouldBe(page2Titles.OrderBy(x => x).ToList());
        
        // Verify page 1 items come before page 2 items in sort order
        if (page1.Data.Any() && page2.Data.Any())
        {
            var lastItemPage1 = page1.Data.Last().Title;
            var firstItemPage2 = page2.Data.First().Title;
            string.Compare(lastItemPage1, firstItemPage2, StringComparison.OrdinalIgnoreCase).ShouldBeLessThanOrEqualTo(0);
        }
    }
}