using Example.LibraryItem.Application;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Validators;

public class ItemLocationValidatorTests
{
    private ItemLocationValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ItemLocationValidator();
    }

    [Test]
    public void Valid_Location_Should_Pass()
    {
    var dto = new ItemLocationDto { Floor = 1, Section = "REF", ShelfCode = "A-125" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Floor_Range_Is_Validated()
    {
    var dto = new ItemLocationDto { Floor = -3, Section = "REF", ShelfCode = "A-125" };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Floor);

    dto = dto with { Floor = 25 };
    _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.Floor);

    dto = dto with { Floor = 5 };
    _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.Floor);
    }

    [Test]
    public void Section_And_ShelfCode_Are_Required_And_Limited()
    {
    var dto = new ItemLocationDto { Floor = 1, Section = "", ShelfCode = "" };
        var result = _validator.TestValidate(dto);
    result.ShouldHaveValidationErrorFor(x => x.Section);
    result.ShouldHaveValidationErrorFor(x => x.ShelfCode);

    dto = dto with { Section = new string('S', 11), ShelfCode = new string('C', 21) };
        result = _validator.TestValidate(dto);
    result.ShouldHaveValidationErrorFor(x => x.Section);
    result.ShouldHaveValidationErrorFor(x => x.ShelfCode);

    dto = dto with { Section = "Fiction", ShelfCode = "F-123" };
        _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
}
