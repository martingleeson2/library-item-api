using System.Net;
using System.Net.Http.Json;
using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Integration;

[TestFixture]
public class ItemsEndpointsValidationTests
{
    private CustomWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new CustomWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task ListItems_WithInvalidPage_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Test invalid page (should be >= 1)
        var response = await client.GetAsync("/v1/items?page=0&limit=10");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ListItems_WithInvalidLimit_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Test invalid limit (should be <= 100)
        var response = await client.GetAsync("/v1/items?page=1&limit=101");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ListItems_WithInvalidSortBy_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Test invalid sort field - "invalid_field" is not in the ValidSortFields list
        var response = await client.GetAsync("/v1/items?page=1&limit=10&sort_by=invalid_field");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ListItems_WithInvalidSortOrder_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Test invalid sort order - "invalid" is not "asc" or "desc"
        var response = await client.GetAsync("/v1/items?page=1&limit=10&sort_by=title&sort_order=invalid");
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateItem_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Test with missing required fields
        var invalidCreate = new ItemCreateRequestDto
        {
            Title = "", // Empty title should fail validation
            ItemType = ItemType.book,
            CallNumber = "",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = null! // Missing location should fail
        };

        var response = await client.PostAsJsonAsync("/v1/items/", invalidCreate, CustomWebApplicationFactory.JsonOptions);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }

    [Test]
    public async Task UpdateItem_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClientWithApiKey();

        // Create a valid item first
        var create = new ItemCreateRequestDto
        {
            Title = "Test Item",
            ItemType = ItemType.book,
            CallNumber = "001",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            Status = ItemStatus.available
        };

        var createResp = await client.PostAsJsonAsync("/v1/items/", create, CustomWebApplicationFactory.JsonOptions);
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>(CustomWebApplicationFactory.JsonOptions);
        Assert.That(created, Is.Not.Null);

        // Now try to update with invalid data
        var invalidUpdate = new ItemUpdateRequestDto
        {
            Title = "", // Empty title should fail validation
            ItemType = ItemType.book,
            CallNumber = "",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            Status = ItemStatus.available
        };

        var response = await client.PutAsJsonAsync($"/v1/items/{created!.Id}", invalidUpdate, CustomWebApplicationFactory.JsonOptions);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.UnprocessableEntity));
    }
}