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
        title = "Valid Title",
        item_type = ItemType.book,
        call_number = "813.52 F553g",
        classification_system = ClassificationSystem.dewey_decimal,
        location = new ItemLocationDto { floor = 1, section = "REF", shelf_code = "A-125" },
        status = ItemStatus.available
    };

    [Test]
    public void Status_Is_Enum()
    {
        var dto = Valid() with { status = (ItemStatus)999 };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.status);

        dto = Valid() with { status = ItemStatus.available };
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.status);
    }

    [Test]
    public void String_Lengths_Are_Limited()
    {
        var dto = Valid() with
        {
            edition = new string('e', 51),
            language = new string('l', 11),
            collection = new string('c', 101),
            barcode = new string('b', 51),
            condition_notes = new string('n', 1001),
            description = new string('d', 2001)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.edition);
        result.ShouldHaveValidationErrorFor(x => x.language);
        result.ShouldHaveValidationErrorFor(x => x.collection);
        result.ShouldHaveValidationErrorFor(x => x.barcode);
        result.ShouldHaveValidationErrorFor(x => x.condition_notes);
        result.ShouldHaveValidationErrorFor(x => x.description);
    }
}
