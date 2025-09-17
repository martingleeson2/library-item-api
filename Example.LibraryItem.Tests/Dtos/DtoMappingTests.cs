using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Dtos;

public class DtoMappingTests
{
    [Test]
    public void ItemDto_Should_Map_From_Item_Entity()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(2, "Fiction", "F-123"),
            Status = ItemStatus.available,
            Isbn = "9780743273565",
            Publisher = "Test Publisher",
            PublicationDate = new DateOnly(2023, 1, 1),
            CreatedAt = new DateTime(2023, 1, 1),
            UpdatedAt = new DateTime(2023, 1, 2)
        };

        // Act
        var dto = item.ToDto("http://localhost");

        // Assert
        dto.ShouldNotBeNull();
        dto.id.ShouldBe(item.Id);
        dto.title.ShouldBe(item.Title);
        dto.item_type.ShouldBe(item.ItemType);
        dto.call_number.ShouldBe(item.CallNumber);
        dto.classification_system.ShouldBe(item.ClassificationSystem);
        dto.status.ShouldBe(item.Status);
        dto.isbn.ShouldBe(item.Isbn);
        dto.publisher.ShouldBe(item.Publisher);
        dto.publication_date.ShouldBe(item.PublicationDate);
        dto.created_at.ShouldBe(item.CreatedAt);
        dto.updated_at.ShouldBe(item.UpdatedAt);
        
        dto.location.ShouldNotBeNull();
        dto.location.floor.ShouldBe(2);
        dto.location.section.ShouldBe("Fiction");
        dto.location.shelf_code.ShouldBe("F-123");
        
        dto._links.ShouldNotBeNull();
        dto._links.self.ShouldNotBeNull();
        dto._links.self.href.ShouldBe($"http://localhost/v1/items/{item.Id}");
    }

    [Test]
    public void ItemCreateRequestDto_Should_Map_To_Item_Entity()
    {
        // Arrange
        var createDto = new ItemCreateRequestDto
        {
            title = "New Book",
            item_type = ItemType.book,
            call_number = "002.42",
            classification_system = ClassificationSystem.dewey_decimal,
            location = new ItemLocationDto { floor = 3, section = "History", shelf_code = "H-456" },
            isbn = "9780743273566",
            publisher = "New Publisher",
            publication_date = new DateOnly(2024, 1, 1)
        };

        var utcNow = DateTime.UtcNow;

        // Act
        var item = createDto.ToEntity(utcNow, "test-user");

        // Assert
        item.ShouldNotBeNull();
        item.Id.ShouldNotBe(Guid.Empty);
        item.Title.ShouldBe(createDto.title);
        item.ItemType.ShouldBe(createDto.item_type);
        item.CallNumber.ShouldBe(createDto.call_number);
        item.ClassificationSystem.ShouldBe(createDto.classification_system);
        item.Status.ShouldBe(ItemStatus.available); // Default status
        item.Isbn.ShouldBe(createDto.isbn);
        item.Publisher.ShouldBe(createDto.publisher);
        item.PublicationDate.ShouldBe(createDto.publication_date);
        item.CreatedBy.ShouldBe("test-user");
        item.UpdatedBy.ShouldBe("test-user");
        
        item.Location.ShouldNotBeNull();
        item.Location.Floor.ShouldBe(3);
        item.Location.Section.ShouldBe("History");
        item.Location.ShelfCode.ShouldBe("H-456");
        
        item.CreatedAt.ShouldBe(utcNow);
        item.UpdatedAt.ShouldBe(utcNow);
    }

    [Test]
    public void ItemUpdateRequestDto_Should_Apply_To_Existing_Item()
    {
        // Arrange
        var existingItem = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "Original", "O-123"),
            Status = ItemStatus.available,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "original-user",
            UpdatedBy = "original-user"
        };

        var updateDto = new ItemUpdateRequestDto
        {
            title = "Updated Title",
            item_type = ItemType.dvd,
            call_number = "002.42",
            classification_system = ClassificationSystem.library_of_congress,
            location = new ItemLocationDto { floor = 2, section = "Updated", shelf_code = "U-456" },
            status = ItemStatus.checked_out,
            isbn = "9780743273567",
            publisher = "Updated Publisher",
            publication_date = new DateOnly(2025, 1, 1)
        };

        var utcNow = DateTime.UtcNow;

        // Act
        existingItem.ApplyUpdate(updateDto, utcNow, "update-user");

        // Assert
        existingItem.Title.ShouldBe(updateDto.title);
        existingItem.ItemType.ShouldBe(updateDto.item_type);
        existingItem.CallNumber.ShouldBe(updateDto.call_number);
        existingItem.ClassificationSystem.ShouldBe(updateDto.classification_system);
        existingItem.Status.ShouldBe(updateDto.status);
        existingItem.Isbn.ShouldBe(updateDto.isbn);
        existingItem.Publisher.ShouldBe(updateDto.publisher);
        existingItem.PublicationDate.ShouldBe(updateDto.publication_date);
        existingItem.UpdatedBy.ShouldBe("update-user");
        
        existingItem.Location.Floor.ShouldBe(2);
        existingItem.Location.Section.ShouldBe("Updated");
        existingItem.Location.ShelfCode.ShouldBe("U-456");
        
        // Should preserve original creation info
        existingItem.CreatedBy.ShouldBe("original-user");
        
        // Should update timestamp
        existingItem.UpdatedAt.ShouldBe(utcNow);
    }

    [Test]
    public void ItemPatchRequestDto_Should_Apply_Only_Non_Null_Values()
    {
        // Arrange
        var existingItem = new Item
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            ItemType = ItemType.book,
            CallNumber = "001.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocation(1, "Original", "O-123"),
            Status = ItemStatus.available,
            Isbn = "9780743273565",
            Publisher = "Original Publisher",
            PublicationDate = new DateOnly(2023, 1, 1),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = "original-user",
            UpdatedBy = "original-user"
        };

        var patchDto = new ItemPatchRequestDto
        {
            title = "Patched Title", // Should update
            // item_type = null, // Should not update
            // call_number = null, // Should not update
            status = ItemStatus.checked_out, // Should update
            isbn = null, // Should not update
            location = new ItemLocationDto { floor = 2, section = "Patched", shelf_code = "P-456" } // Should update
            // Other fields null - should not update
        };

        var utcNow = DateTime.UtcNow;

        // Act
        existingItem.ApplyPatch(patchDto, utcNow, "patch-user");

        // Assert
        // Updated fields
        existingItem.Title.ShouldBe("Patched Title");
        existingItem.Status.ShouldBe(ItemStatus.checked_out);
        existingItem.Location.Floor.ShouldBe(2);
        existingItem.Location.Section.ShouldBe("Patched");
        existingItem.Location.ShelfCode.ShouldBe("P-456");
        existingItem.UpdatedBy.ShouldBe("patch-user");
        
        // Unchanged fields
        existingItem.ItemType.ShouldBe(ItemType.book);
        existingItem.CallNumber.ShouldBe("001.42");
        existingItem.ClassificationSystem.ShouldBe(ClassificationSystem.dewey_decimal);
        existingItem.Isbn.ShouldBe("9780743273565");
        existingItem.Publisher.ShouldBe("Original Publisher");
        existingItem.PublicationDate.ShouldBe(new DateOnly(2023, 1, 1));
        
        // Should preserve original creation info
        existingItem.CreatedBy.ShouldBe("original-user");
        
        // Should update timestamp
        existingItem.UpdatedAt.ShouldBe(utcNow);
    }
}