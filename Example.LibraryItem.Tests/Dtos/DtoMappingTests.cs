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
        dto.Id.ShouldBe(item.Id);
        dto.Title.ShouldBe(item.Title);
        dto.ItemType.ShouldBe(item.ItemType);
        dto.CallNumber.ShouldBe(item.CallNumber);
        dto.ClassificationSystem.ShouldBe(item.ClassificationSystem);
        dto.Status.ShouldBe(item.Status);
        dto.Isbn.ShouldBe(item.Isbn);
        dto.Publisher.ShouldBe(item.Publisher);
        dto.PublicationDate.ShouldBe(item.PublicationDate);
        dto.CreatedAt.ShouldBe(item.CreatedAt);
        dto.UpdatedAt.ShouldBe(item.UpdatedAt);
        
        dto.Location.ShouldNotBeNull();
        dto.Location.Floor.ShouldBe(2);
        dto.Location.Section.ShouldBe("Fiction");
        dto.Location.ShelfCode.ShouldBe("F-123");
    }

    [Test]
    public void ItemCreateRequestDto_Should_Map_To_Item_Entity()
    {
        // Arrange
        var createDto = new ItemCreateRequestDto
        {
            Title = "New Book",
            ItemType = ItemType.book,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 3, Section = "History", ShelfCode = "H-456" },
            Isbn = "9780743273566",
            Publisher = "New Publisher",
            PublicationDate = new DateOnly(2024, 1, 1)
        };

        var utcNow = DateTime.UtcNow;

        // Act
        var item = createDto.ToEntity(utcNow, "test-user");

        // Assert
        item.ShouldNotBeNull();
        item.Id.ShouldNotBe(Guid.Empty);
        item.Title.ShouldBe(createDto.Title);
        item.ItemType.ShouldBe(createDto.ItemType);
        item.CallNumber.ShouldBe(createDto.CallNumber);
        item.ClassificationSystem.ShouldBe(createDto.ClassificationSystem);
        item.Status.ShouldBe(ItemStatus.available); // Default status
        item.Isbn.ShouldBe(createDto.Isbn);
        item.Publisher.ShouldBe(createDto.Publisher);
        item.PublicationDate.ShouldBe(createDto.PublicationDate);
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
            Title = "Updated Title",
            ItemType = ItemType.dvd,
            CallNumber = "002.42",
            ClassificationSystem = ClassificationSystem.library_of_congress,
            Location = new ItemLocationDto { Floor = 2, Section = "Updated", ShelfCode = "U-456" },
            Status = ItemStatus.checked_out,
            Isbn = "9780743273567",
            Publisher = "Updated Publisher",
            PublicationDate = new DateOnly(2025, 1, 1)
        };

        var utcNow = DateTime.UtcNow;

        // Act
        existingItem.ApplyUpdate(updateDto, utcNow, "update-user");

        // Assert
        existingItem.Title.ShouldBe(updateDto.Title);
        existingItem.ItemType.ShouldBe(updateDto.ItemType);
        existingItem.CallNumber.ShouldBe(updateDto.CallNumber);
        existingItem.ClassificationSystem.ShouldBe(updateDto.ClassificationSystem);
        existingItem.Status.ShouldBe(updateDto.Status);
        existingItem.Isbn.ShouldBe(updateDto.Isbn);
        existingItem.Publisher.ShouldBe(updateDto.Publisher);
        existingItem.PublicationDate.ShouldBe(updateDto.PublicationDate);
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
            Title = "Patched Title", // Should update
            // ItemType = null, // Should not update
            // CallNumber = null, // Should not update
            Status = ItemStatus.checked_out, // Should update
            Isbn = null, // Should not update
            Location = new ItemLocationDto { Floor = 2, Section = "Patched", ShelfCode = "P-456" } // Should update
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