using System.Net;
using System.Net.Http.Json;
using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Integration;

[TestFixture]
public class IsolatedSeededEndpointsTests
{
    [Test]
    public async Task Seeded_Get_Returns200()
    {
        using var factory = new CustomWebApplicationFactory();
        // Seed a single item directly into the unique in-memory DB
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var now = DateTime.UtcNow;
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Seeded Item",
            Author = "Tester",
            ItemType = ItemType.book,
            CallNumber = "000.1 TEST",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "T", "S1"),
            Status = ItemStatus.available,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "test-user",
            UpdatedBy = "test-user"
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var client = factory.CreateClientWithApiKey();
        var resp = await client.GetAsync($"/v1/items/{item.Id}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dto = await resp.Content.ReadFromJsonAsync<ItemDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.Title, Is.EqualTo("Seeded Item"));
    }

    [Test]
    public async Task Seeded_Put_Returns200()
    {
        using var factory = new CustomWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var now = DateTime.UtcNow;
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "To Update",
            Author = "Tester",
            ItemType = ItemType.book,
            CallNumber = "000.2 TEST",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "U", "S2"),
            Status = ItemStatus.available,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "test-user",
            UpdatedBy = "test-user"
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var client = factory.CreateClientWithApiKey();
        var update = new ItemUpdateRequestDto
        {
            Title = "Updated Title",
            Author = item.Author,
            ItemType = item.ItemType,
            CallNumber = item.CallNumber,
            ClassificationSystem = item.ClassificationSystem,
            Location = new ItemLocationDto { Floor = 1, Section = "U", ShelfCode = "S2" },
            Status = ItemStatus.available
        };
        var resp = await client.PutAsJsonAsync($"/v1/items/{item.Id}", update, CustomWebApplicationFactory.JsonOptions);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await resp.Content.ReadFromJsonAsync<ItemDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
    }

    [Test]
    public async Task Seeded_Post_Returns201()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClientWithApiKey();

        var create = new ItemCreateRequestDto
        {
            Title = "Posted Item",
            Author = "Poster",
            ItemType = ItemType.book,
            CallNumber = "100.1 POST",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "P", ShelfCode = "P1" },
            Isbn = "9780000000000",
            Status = ItemStatus.available
        };

        var resp = await client.PostAsJsonAsync("/v1/items/", create, CustomWebApplicationFactory.JsonOptions);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await resp.Content.ReadFromJsonAsync<ItemDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.Title, Is.EqualTo("Posted Item"));
    }

    [Test]
    public async Task Seeded_Delete_Returns204()
    {
        using var factory = new CustomWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var now = DateTime.UtcNow;
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "To Delete",
            Author = "Tester",
            ItemType = ItemType.book,
            CallNumber = "200.1 DEL",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "D", "S3"),
            Status = ItemStatus.available,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = "test-user",
            UpdatedBy = "test-user"
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var client = factory.CreateClientWithApiKey();
        var del = await client.DeleteAsync($"/v1/items/{item.Id}");
        Assert.That(del.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getAfter = await client.GetAsync($"/v1/items/{item.Id}");
        Assert.That(getAfter.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
