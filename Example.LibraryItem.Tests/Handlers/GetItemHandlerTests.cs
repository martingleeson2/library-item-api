using Example.LibraryItem.Application.Handlers;
using Example.LibraryItem.Domain;
using Example.LibraryItem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Handlers;

public class GetItemHandlerTests
{
    private LibraryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LibraryDbContext(options);
    }

    [Test]
    public async Task Returns_Item_When_Found()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<GetItemHandler>>();
        var itemId = Guid.NewGuid();
        var item = new Item 
        { 
            Id = itemId, 
            Title = "Test Book", 
            ItemType = ItemType.book, 
            CallNumber = "001.42", 
            ClassificationSystem = ClassificationSystem.dewey_decimal, 
            Location = new ItemLocation(1, "A", "B"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        var handler = new GetItemHandler(db, logger);

        // Act
        var result = await handler.HandleAsync(itemId, "http://localhost");

        // Assert
        result.ShouldNotBeNull();
        result.id.ShouldBe(itemId);
        result.title.ShouldBe("Test Book");
    }

    [Test]
    public async Task Returns_Null_When_Not_Found()
    {
        // Arrange
        using var db = CreateDb();
        var logger = Mock.Of<ILogger<GetItemHandler>>();
        var handler = new GetItemHandler(db, logger);

        // Act
        var result = await handler.HandleAsync(Guid.NewGuid(), "http://localhost");

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task Logs_When_Item_Not_Found()
    {
        // Arrange
        using var db = CreateDb();
        var mockLogger = new Mock<ILogger<GetItemHandler>>();
        var handler = new GetItemHandler(db, mockLogger.Object);
        var itemId = Guid.NewGuid();

        // Act
        await handler.HandleAsync(itemId, "http://localhost");

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((object)v) != null && v.ToString()!.Contains($"Item {itemId} not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}