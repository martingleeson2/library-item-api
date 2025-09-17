using System.Net;
using System.Net.Http.Json;
using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Integration;

[TestFixture]
public class ItemsEndpointsHappyPathTests
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
    public async Task Create_Get_List_Update_Patch_Delete_Flow_Works()
    {
        var client = _factory.CreateClientWithApiKey();

        var create = new ItemCreateRequestDto
        {
            title = "The Pragmatic Programmer",
            author = "Andrew Hunt",
            item_type = ItemType.book,
            call_number = "005.1 HUN",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 1, section = "CS", shelf_code = "CS-01", wing = "A", position = "3", notes = null },
            isbn = "9780135957059",
            publisher = "Addison-Wesley",
            pages = 352,
            language = "en"
        };

        var createResp = await client.PostAsJsonAsync("/v1/items/", create);
        Assert.That(createResp.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(created, Is.Not.Null);
        Assert.That(created!.id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(created.title, Is.EqualTo(create.title));

        var id = created.id;

        var getResp = await client.GetAsync($"/v1/items/{id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(fetched, Is.Not.Null);
        Assert.That(fetched!.id, Is.EqualTo(id));
        Assert.That(fetched._links?.self?.href, Does.Contain(id.ToString()));

        var listResp = await client.GetAsync("/v1/items?page=1&limit=10");
        Assert.That(listResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var list = await listResp.Content.ReadFromJsonAsync<ItemListResponseDto>();
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.data.Count, Is.GreaterThanOrEqualTo(1));
        Assert.That(list.pagination.page, Is.EqualTo(1));

        var update = new ItemUpdateRequestDto
        {
            title = "The Pragmatic Programmer (Updated)",
            author = "Andrew Hunt",
            item_type = ItemType.book,
            call_number = "005.1 HUN",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 1, section = "CS", shelf_code = "CS-01", wing = "A", position = "4", notes = null },
            status = ItemStatus.available,
            isbn = "9780135957059",
            publisher = "Addison-Wesley",
            pages = 360,
            language = "en"
        };
        var updateResp = await client.PutAsJsonAsync($"/v1/items/{id}", update);
        Assert.That(updateResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await updateResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.title, Is.EqualTo(update.title));
        Assert.That(updated.pages, Is.EqualTo(update.pages));

        var patch = new ItemPatchRequestDto
        {
            subtitle = "20th Anniversary",
            pages = 365
        };
        var patchResp = await client.PatchAsJsonAsync($"/v1/items/{id}", patch);
        Assert.That(patchResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var patched = await patchResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(patched, Is.Not.Null);
        Assert.That(patched!.subtitle, Is.EqualTo("20th Anniversary"));
        Assert.That(patched.pages, Is.EqualTo(365));

        var deleteResp = await client.DeleteAsync($"/v1/items/{id}");
        Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getAfterDelete = await client.GetAsync($"/v1/items/{id}");
        Assert.That(getAfterDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
