using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Validators;

public class ItemUpdateValidatorTests
{
    private ItemUpdateValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ItemUpdateValidator();
    }

    private static ItemUpdateRequestDto Valid() => new()
    {
    Title = "Valid Title",
    ItemType = ItemType.book,
    CallNumber = "813.52 F553g",
    ClassificationSystem = ClassificationSystem.dewey_decimal,
    Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" },
    Status = ItemStatus.available
    };

    [Test]
    public void Status_Is_Enum()
    {
    var dto = Valid() with { Status = (ItemStatus)999 };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Status);

    dto = Valid() with { Status = ItemStatus.available };
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Test]
    public void String_Lengths_Are_Limited()
    {
        var dto = Valid() with
        {
            Edition = new string('e', 51),
            Language = new string('l', 11),
            Collection = new string('c', 101),
            Barcode = new string('b', 51),
            ConditionNotes = new string('n', 1001),
            Description = new string('d', 2001)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Edition);
        result.ShouldHaveValidationErrorFor(x => x.Language);
        result.ShouldHaveValidationErrorFor(x => x.Collection);
        result.ShouldHaveValidationErrorFor(x => x.Barcode);
        result.ShouldHaveValidationErrorFor(x => x.ConditionNotes);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
