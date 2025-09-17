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
            Title = "The Pragmatic Programmer",
            Author = "Andrew Hunt",
            ItemType = ItemType.book,
            CallNumber = "005.1 HUN",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "CS", ShelfCode = "CS-01", Wing = "A", Position = "3", Notes = null },
            Isbn = "9780135957059",
            Publisher = "Addison-Wesley",
            Pages = 352,
            Language = "en"
        };

        var createResp = await client.PostAsJsonAsync("/v1/items/", create);
        Assert.That(createResp.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        var created = await createResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(created, Is.Not.Null);
    Assert.That(created!.Id, Is.Not.EqualTo(Guid.Empty));
    Assert.That(created.Title, Is.EqualTo(create.Title));

    var id = created.Id;

        var getResp = await client.GetAsync($"/v1/items/{id}");
        Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var fetched = await getResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(fetched, Is.Not.Null);
    Assert.That(fetched!.Id, Is.EqualTo(id));

        var listResp = await client.GetAsync("/v1/items?page=1&limit=10");
        Assert.That(listResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var list = await listResp.Content.ReadFromJsonAsync<ItemListResponseDto>();
        Assert.That(list, Is.Not.Null);
    Assert.That(list!.Data.Count, Is.GreaterThanOrEqualTo(1));
    Assert.That(list.Pagination.Page, Is.EqualTo(1));

        var update = new ItemUpdateRequestDto
        {
            Title = "The Pragmatic Programmer (Updated)",
            Author = "Andrew Hunt",
            ItemType = ItemType.book,
            CallNumber = "005.1 HUN",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "CS", ShelfCode = "CS-01", Wing = "A", Position = "4", Notes = null },
            Status = ItemStatus.available,
            Isbn = "9780135957059",
            Publisher = "Addison-Wesley",
            Pages = 360,
            Language = "en"
        };
        var updateResp = await client.PutAsJsonAsync($"/v1/items/{id}", update);
        Assert.That(updateResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await updateResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(updated, Is.Not.Null);
    Assert.That(updated!.Title, Is.EqualTo(update.Title));
    Assert.That(updated.Pages, Is.EqualTo(update.Pages));

        var patch = new ItemPatchRequestDto
        {
            Subtitle = "20th Anniversary",
            Pages = 365
        };
        var patchResp = await client.PatchAsJsonAsync($"/v1/items/{id}", patch);
        Assert.That(patchResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var patched = await patchResp.Content.ReadFromJsonAsync<ItemDto>();
        Assert.That(patched, Is.Not.Null);
    Assert.That(patched!.Subtitle, Is.EqualTo("20th Anniversary"));
    Assert.That(patched.Pages, Is.EqualTo(365));

        var deleteResp = await client.DeleteAsync($"/v1/items/{id}");
        Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var getAfterDelete = await client.GetAsync($"/v1/items/{id}");
        Assert.That(getAfterDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
