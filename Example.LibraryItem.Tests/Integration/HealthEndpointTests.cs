using System.Net;
using System.Text.Json;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Integration;

[TestFixture]
public class HealthEndpointTests
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
    public async Task Health_WithApiKey_Returns_Ok()
    {
        var client = _factory.CreateClientWithApiKey();
        var resp = await client.GetAsync("/health");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
    Assert.That(doc.RootElement.TryGetProperty("status", out var statusProp), Is.True);
    Assert.That(statusProp.GetString(), Is.EqualTo("healthy"));
    Assert.That(doc.RootElement.TryGetProperty("timestamp", out _), Is.True);
    Assert.That(doc.RootElement.TryGetProperty("results", out _), Is.True);
    }

    [Test]
    public async Task Health_WithoutApiKey_Returns_Unauthorized()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/health");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
