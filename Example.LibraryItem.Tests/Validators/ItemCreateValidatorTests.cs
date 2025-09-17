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
        Title = "Valid Title",
        ItemType = ItemType.book,
        CallNumber = "813.52 F553g",
        ClassificationSystem = ClassificationSystem.dewey_decimal,
        Location = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" }
    };

    [Test]
    public void Title_Is_Required_And_Max500()
    {
    var dto = Valid() with { Title = "" };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Title);

    dto = Valid() with { Title = new string('A', 501) };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Title);

    dto = Valid();
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void CallNumber_Is_Required_And_Max50()
    {
    var dto = Valid() with { CallNumber = "" };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CallNumber);

    dto = Valid() with { CallNumber = new string('1', 51) };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.CallNumber);

    dto = Valid() with { CallNumber = "001.42" };
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.CallNumber);
    }

    [Test]
    public void Location_Is_Required_And_Validated()
    {
        var dto = Valid() with { Location = null! };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Location);

    dto = Valid() with { Location = new ItemLocationDto { Floor = -2, Section = "", ShelfCode = "" } };
        var result = _validator.TestValidate(dto);
    // floor -2 is valid per validator; only section/shelf_code should error
        result.ShouldHaveValidationErrorFor("Location.Section");
        result.ShouldHaveValidationErrorFor("Location.ShelfCode");
    }

    [Test]
    public void Isbn_And_Issn_Format_Are_Validated_When_Present()
    {
    var dto = Valid() with { Isbn = "invalid" };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Isbn);

    dto = Valid() with { Isbn = "9780743273565" };
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Isbn);

    dto = Valid() with { Issn = "invalid" };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Issn);

    dto = Valid() with { Issn = "0317-8471" };
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Issn);
    }

    [Test]
    public void Optional_String_Lengths_Are_Limited()
    {
        var dto = Valid() with
        {
            Publisher = new string('p', 256),
            Edition = new string('e', 51),
            Language = new string('l', 11),
            Collection = new string('c', 101),
            Barcode = new string('b', 51),
            ConditionNotes = new string('n', 1001),
            Description = new string('d', 2001)
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Publisher);
        result.ShouldHaveValidationErrorFor(x => x.Edition);
        result.ShouldHaveValidationErrorFor(x => x.Language);
        result.ShouldHaveValidationErrorFor(x => x.Collection);
        result.ShouldHaveValidationErrorFor(x => x.Barcode);
        result.ShouldHaveValidationErrorFor(x => x.ConditionNotes);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
