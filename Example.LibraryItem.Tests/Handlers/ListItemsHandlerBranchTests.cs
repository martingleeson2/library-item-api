using System.Linq;
using Example.LibraryItem.Application;
using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Example.LibraryItem.Tests.Helpers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class ListItemsHandlerBranchTests
{
    private static (LibraryDbContext db, ListItemsHandler handler) Create()
    {
        var db = TestHelpers.CreateInMemoryDb();
        var handler = new ListItemsHandler(db, Moq.Mock.Of<ILogger<ListItemsHandler>>());
        return (db, handler);
    }

    private static void Seed(LibraryDbContext db)
    {
        var now = DateTime.UtcNow;
        db.Items.AddRange(new[]
        {
            new Item { Id = Guid.NewGuid(), Title = "Alpha", Author = "AuthA", Isbn = "1111111111", ItemType = ItemType.book, CallNumber = "001A", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Collection = "Gen", Location = new ItemLocation(1, "A1", "S1"), PublicationDate = new DateOnly(2020,1,1), CreatedAt = now.AddMinutes(1), UpdatedAt = now.AddMinutes(3) },
            new Item { Id = Guid.NewGuid(), Title = "Beta", Author = "AuthB", Isbn = "2222222222", ItemType = ItemType.book, CallNumber = "002B", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.checked_out, Collection = "Spec", Location = new ItemLocation(2, "B2", "S2"), PublicationDate = new DateOnly(2021,1,1), CreatedAt = now.AddMinutes(2), UpdatedAt = now.AddMinutes(2) },
            new Item { Id = Guid.NewGuid(), Title = "Gamma", Author = "AuthC", Isbn = "3333333333", ItemType = ItemType.dvd, CallNumber = "003C", ClassificationSystem = ClassificationSystem.library_of_congress, Status = ItemStatus.available, Collection = "Gen", Location = new ItemLocation(3, "C3", "S3"), PublicationDate = new DateOnly(2019,1,1), CreatedAt = now.AddMinutes(4), UpdatedAt = now.AddMinutes(1) },
        });
        db.SaveChanges();
    }

    [Test]
    public async Task Sorts_Descending_By_Title()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var query = new ListItemsQuery(
                Page: 1, Limit: 10,
                Title: null, Author: null, Isbn: null,
                ItemType: null, Status: null, Collection: null,
                LocationFloor: null, LocationSection: null, CallNumber: null,
                PublicationYearFrom: null, PublicationYearTo: null,
                SortBy: "title", SortOrder: "desc");
            var result = await handler.HandleAsync(query, default);
            result.Data.Select(i => i.Title).ShouldBe(new[] { "Gamma", "Beta", "Alpha" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Defaults_To_Title_When_Invalid_SortBy()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var query = new ListItemsQuery(
                Page: 1, Limit: 10,
                Title: null, Author: null, Isbn: null,
                ItemType: null, Status: null, Collection: null,
                LocationFloor: null, LocationSection: null, CallNumber: null,
                PublicationYearFrom: null, PublicationYearTo: null,
                SortBy: "doesnotexist", SortOrder: null);
            var result = await handler.HandleAsync(query, default);
            result.Data.Select(i => i.Title).ShouldBe(new[] { "Alpha", "Beta", "Gamma" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Filters_By_Status_And_Collection()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var query = new ListItemsQuery(
                Page: 1, Limit: 10,
                Title: null, Author: null, Isbn: null,
                ItemType: null, Status: ItemStatus.available, Collection: "Gen",
                LocationFloor: null, LocationSection: null, CallNumber: null,
                PublicationYearFrom: null, PublicationYearTo: null,
                SortBy: null, SortOrder: null);
            var result = await handler.HandleAsync(query, default);
            result.Data.Count.ShouldBe(2);
            result.Data.All(i => i.Status == ItemStatus.available && i.Collection == "Gen").ShouldBeTrue();
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Filters_By_Title_And_Author_Contains()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var titleQuery = new ListItemsQuery(1, 10, "a", null, null, null, null, null, null, null, null, null, null, null, null);
            var titleRes = await handler.HandleAsync(titleQuery, default);
            titleRes.Data.Select(d => d.Title).ShouldBe(new[] { "Alpha", "Beta", "Gamma" });

            var authorQuery = new ListItemsQuery(1, 10, null, "AuthB", null, null, null, null, null, null, null, null, null, null, null);
            var authorRes = await handler.HandleAsync(authorQuery, default);
            authorRes.Data.Select(d => d.Title).ShouldBe(new[] { "Beta" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Filters_By_Isbn_Exact_And_CallNumber_Contains()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var isbnQuery = new ListItemsQuery(1, 10, null, null, "2222222222", null, null, null, null, null, null, null, null, null, null);
            var isbnRes = await handler.HandleAsync(isbnQuery, default);
            isbnRes.Data.Select(d => d.Title).ShouldBe(new[] { "Beta" });

            var callQuery = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, "003", null, null, null, null);
            var callRes = await handler.HandleAsync(callQuery, default);
            callRes.Data.Select(d => d.Title).ShouldContain("Gamma");
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Filters_By_Location_And_Publication_Year_Range()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var locQuery = new ListItemsQuery(1, 10, null, null, null, null, null, null, 2, "B2", null, null, null, null, null);
            var locRes = await handler.HandleAsync(locQuery, default);
            locRes.Data.Select(d => d.Title).ShouldBe(new[] { "Beta" });

            var yearFromQuery = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, 2020, null, null, null);
            var yearFromRes = await handler.HandleAsync(yearFromQuery, default);
            yearFromRes.Data.Select(d => d.Title).ShouldBe(new[] { "Alpha", "Beta" });

            var yearToQuery = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, 2020, null, null);
            var yearToRes = await handler.HandleAsync(yearToQuery, default);
            yearToRes.Data.Select(d => d.Title).ShouldBe(new[] { "Alpha", "Gamma" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Sorts_By_Author_Ascending_And_Descending()
    {
        var (db, handler) = Create();
        try
        {
            db.Items.AddRange(new[]
            {
                new Item { Id = Guid.NewGuid(), Title = "Z", Author = "Carl", ItemType = ItemType.book, CallNumber = "010", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Collection = "Gen", Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "Y", Author = "Alice", ItemType = ItemType.book, CallNumber = "011", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Collection = "Gen", Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "X", Author = "Bob", ItemType = ItemType.book, CallNumber = "012", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Collection = "Gen", Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            db.SaveChanges();

            var asc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "author", null);
            var ascResult = await handler.HandleAsync(asc, default);
            ascResult.Data.Select(d => d.Author).ShouldBe(new[] { "Alice", "Bob", "Carl" });

            var desc = asc with { SortOrder = "desc" };
            var descResult = await handler.HandleAsync(desc, default);
            descResult.Data.Select(d => d.Author).ShouldBe(new[] { "Carl", "Bob", "Alice" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Sorts_By_PublicationDate_And_CallNumber()
    {
        var (db, handler) = Create();
        try
        {
            db.Items.AddRange(new[]
            {
                new Item { Id = Guid.NewGuid(), Title = "P1", PublicationDate = new DateOnly(2020,1,1), CallNumber = "100", ItemType = ItemType.book, ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "P2", PublicationDate = new DateOnly(2019,1,1), CallNumber = "101", ItemType = ItemType.book, ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "P3", PublicationDate = new DateOnly(2021,1,1), CallNumber = "099", ItemType = ItemType.book, ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            db.SaveChanges();

            var pubDesc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "publication_date", "desc");
            var pubRes = await handler.HandleAsync(pubDesc, default);
            pubRes.Data.Select(d => d.Title).ShouldBe(new[] { "P3", "P1", "P2" });

            var callDesc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "call_number", "desc");
            var callRes = await handler.HandleAsync(callDesc, default);
            callRes.Data.Select(d => d.CallNumber).ShouldBe(new[] { "101", "100", "099" }, Case.Insensitive);
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Sorts_By_CreatedAt_And_UpdatedAt()
    {
        var (db, handler) = Create();
        try
        {
            // Seed already includes different CreatedAt/UpdatedAt values
            Seed(db);

            var createdAsc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "created_at", null);
            var createdRes = await handler.HandleAsync(createdAsc, default);
            createdRes.Data.Select(d => d.Title).ShouldBe(new[] { "Alpha", "Beta", "Gamma" });

            var updatedDesc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "updated_at", "desc");
            var updatedRes = await handler.HandleAsync(updatedDesc, default);
            updatedRes.Data.Select(d => d.Title).ShouldBe(new[] { "Alpha", "Beta", "Gamma" });
        }
        finally
        {
            db.Dispose();
        }
    }

    [Test]
    public async Task Sorts_By_Author_Asc_And_Desc()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var asc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "author", "asc");
            var ascResult = await handler.HandleAsync(asc, default);
            ascResult.Data.Select(i => i.Author).ShouldBe(new[] { "AuthA", "AuthB", "AuthC" });

            var desc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "author", "desc");
            var descResult = await handler.HandleAsync(desc, default);
            descResult.Data.Select(i => i.Author).ShouldBe(new[] { "AuthC", "AuthB", "AuthA" });
        }
        finally { db.Dispose(); }
    }

    [Test]
    public async Task Sorts_By_PublicationDate_Asc_And_Desc()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var asc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "publication_date", "asc");
            var ascYears = (await handler.HandleAsync(asc, default)).Data.Select(i => i.PublicationDate?.Year);
            ascYears.ShouldBe(new int?[] { 2019, 2020, 2021 });

            var desc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "publication_date", "desc");
            var descYears = (await handler.HandleAsync(desc, default)).Data.Select(i => i.PublicationDate?.Year);
            descYears.ShouldBe(new int?[] { 2021, 2020, 2019 });
        }
        finally { db.Dispose(); }
    }

    [Test]
    public async Task Sorts_By_CallNumber_And_Timestamps()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            var callDesc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "call_number", "desc");
            (await handler.HandleAsync(callDesc, default)).Data.Select(i => i.CallNumber).ShouldBe(new[] { "003C", "002B", "001A" });

            var createdDesc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "created_at", "desc");
            (await handler.HandleAsync(createdDesc, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Gamma", "Beta", "Alpha" });

            var updatedAsc = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "updated_at", "asc");
            (await handler.HandleAsync(updatedAsc, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Gamma", "Beta", "Alpha" });
        }
        finally { db.Dispose(); }
    }

    [Test]
    public async Task Filters_Title_Author_Isbn_ItemType_Location_PublicationRange()
    {
        var (db, handler) = Create();
        try
        {
            Seed(db);
            // Title exact contains -> 1
            var byTitle = new ListItemsQuery(1, 10, "Beta", null, null, null, null, null, null, null, null, null, null, null, null);
            (await handler.HandleAsync(byTitle, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Beta" });

            // Author contains
            var byAuthor = new ListItemsQuery(1, 10, null, "AuthA", null, null, null, null, null, null, null, null, null, null, null);
            (await handler.HandleAsync(byAuthor, default)).Data.Select(i => i.Author).ShouldBe(new[] { "AuthA" });

            // ISBN exact
            var byIsbn = new ListItemsQuery(1, 10, null, null, "3333333333", null, null, null, null, null, null, null, null, null, null);
            (await handler.HandleAsync(byIsbn, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Gamma" });

            // ItemType
            var byType = new ListItemsQuery(1, 10, null, null, null, ItemType.dvd, null, null, null, null, null, null, null, null, null);
            (await handler.HandleAsync(byType, default)).Data.All(i => i.ItemType == ItemType.dvd).ShouldBeTrue();

            // Location floor and section
            var byFloor = new ListItemsQuery(1, 10, null, null, null, null, null, null, 2, null, null, null, null, null, null);
            (await handler.HandleAsync(byFloor, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Beta" });

            var bySection = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, "C3", null, null, null, null, null);
            (await handler.HandleAsync(bySection, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Gamma" });

            // Call number contains
            var byCall = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, "003", null, null, null, null);
            (await handler.HandleAsync(byCall, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Gamma" });

            // Publication year range
            var byFromTo = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, 2020, 2021, null, null);
            (await handler.HandleAsync(byFromTo, default)).Data.Select(i => i.Title).ShouldBe(new[] { "Alpha", "Beta" });
        }
        finally { db.Dispose(); }
    }

    [Test]
    public async Task Filters_By_Author_With_Null_Author_Items()
    {
        var (db, handler) = Create();
        try
        {
            // Add items with and without authors to test the null check branch
            db.Items.AddRange(new[]
            {
                new Item { Id = Guid.NewGuid(), Title = "WithAuthor", Author = "TestAuthor", ItemType = ItemType.book, CallNumber = "001", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "NoAuthor", Author = null, ItemType = ItemType.book, CallNumber = "002", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            db.SaveChanges();

            // Filter by author should only return items where Author is not null AND contains the search term
            var query = new ListItemsQuery(1, 10, null, "Test", null, null, null, null, null, null, null, null, null, null, null);
            var result = await handler.HandleAsync(query, default);
            
            result.Data.Count.ShouldBe(1);
            result.Data[0].Title.ShouldBe("WithAuthor");
            result.Data[0].Author.ShouldBe("TestAuthor");
        }
        finally { db.Dispose(); }
    }

    [Test]
    public async Task Filters_By_PublicationYear_With_Null_PublicationDate_Items()
    {
        var (db, handler) = Create();
        try
        {
            // Add items with and without publication dates to test the null check branches
            db.Items.AddRange(new[]
            {
                new Item { Id = Guid.NewGuid(), Title = "WithDate", PublicationDate = new DateOnly(2020, 1, 1), ItemType = ItemType.book, CallNumber = "001", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new Item { Id = Guid.NewGuid(), Title = "NoDate", PublicationDate = null, ItemType = ItemType.book, CallNumber = "002", ClassificationSystem = ClassificationSystem.dewey_decimal, Status = ItemStatus.available, Location = new ItemLocation(1, "A", "B"), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            });
            db.SaveChanges();

            // Filter by publication year range - should only return items where PublicationDate is not null
            var queryFrom = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, 2019, null, null, null);
            var resultFrom = await handler.HandleAsync(queryFrom, default);
            resultFrom.Data.Count.ShouldBe(1);
            resultFrom.Data[0].Title.ShouldBe("WithDate");

            var queryTo = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, 2021, null, null);
            var resultTo = await handler.HandleAsync(queryTo, default);
            resultTo.Data.Count.ShouldBe(1);
            resultTo.Data[0].Title.ShouldBe("WithDate");
        }
        finally { db.Dispose(); }
    }
}
