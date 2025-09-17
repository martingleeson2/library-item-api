using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Validators;

public class ItemCreateValidatorTests
{
    private ItemCreateValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ItemCreateValidator();
    }

    private static ItemCreateRequestDto Valid() => new()
    {
        title = "Valid Title",
        item_type = ItemType.book,
        call_number = "813.52 F553g",
        classification_system = ClassificationSystem.dewey_decimal,
        location = new ItemLocationDto { floor = 1, section = "REF", shelf_code = "A-125" }
    };

    [Test]
    public void Title_Is_Required_And_Max500()
    {
        var dto = Valid() with { title = "" };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.title);

        dto = Valid() with { title = new string('A', 501) };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.title);

        dto = Valid();
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.title);
    }

    [Test]
    public void CallNumber_Is_Required_And_Max50()
    {
        var dto = Valid() with { call_number = "" };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.call_number);

        dto = Valid() with { call_number = new string('1', 51) };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.call_number);

        dto = Valid() with { call_number = "001.42" };
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.call_number);
    }

    [Test]
    public void Location_Is_Required_And_Validated()
    {
        var dto = Valid() with { location = null! };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.location);

    dto = Valid() with { location = new ItemLocationDto { floor = -2, section = "", shelf_code = "" } };
        var result = _validator.TestValidate(dto);
    // floor -2 is valid per validator; only section/shelf_code should error
        result.ShouldHaveValidationErrorFor("location.section");
        result.ShouldHaveValidationErrorFor("location.shelf_code");
    }

    [Test]
    public void Isbn_And_Issn_Format_Are_Validated_When_Present()
    {
        var dto = Valid() with { isbn = "invalid" };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.isbn);

        dto = Valid() with { isbn = "9780743273565" };
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.isbn);

        dto = Valid() with { issn = "invalid" };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.issn);

        dto = Valid() with { issn = "0317-8471" };
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.issn);
    }

    [Test]
    public void Optional_String_Lengths_Are_Limited()
    {
        var dto = Valid() with
        {
            publisher = new string('p', 256),
            edition = new string('e', 51),
            language = new string('l', 11),
            collection = new string('c', 101),
            barcode = new string('b', 51),
            condition_notes = new string('n', 1001),
            description = new string('d', 2001)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.publisher);
        result.ShouldHaveValidationErrorFor(x => x.edition);
        result.ShouldHaveValidationErrorFor(x => x.language);
        result.ShouldHaveValidationErrorFor(x => x.collection);
        result.ShouldHaveValidationErrorFor(x => x.barcode);
        result.ShouldHaveValidationErrorFor(x => x.condition_notes);
        result.ShouldHaveValidationErrorFor(x => x.description);
    }
}
