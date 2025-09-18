using System.Net;
using System.Net.Http.Json;
using Example.LibraryItem.Application;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Integration;

[TestFixture]
public class ProgramStartupTests
{
    private DevelopmentWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new DevelopmentWebApplicationFactory();
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();
    }

    [Test]
    public async Task Development_EnablesSwaggerUI()
    {
        var client = _factory.CreateClient();

        var swagger = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.That(swagger.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var ui = await client.GetAsync("/swagger/index.html");
        Assert.That((int)ui.StatusCode, Is.InRange(200, 399));
    }

    [Test]
    public async Task Development_SeedsInitialItems()
    {
        var client = _factory.CreateDevClientWithApiKey();
        var listResp = await client.GetAsync("/v1/items?page=1&limit=1");
        Assert.That(listResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var list = await listResp.Content.ReadFromJsonAsync<ItemListResponseDto>();
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Data.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(list.Pagination.TotalItems, Is.GreaterThanOrEqualTo(1));
    }
}
