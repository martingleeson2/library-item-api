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
        var dto = new ItemLocationDto { floor = 1, section = "REF", shelf_code = "A-125" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Floor_Range_Is_Validated()
    {
        var dto = new ItemLocationDto { floor = -3, section = "REF", shelf_code = "A-125" };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.floor);

        dto = dto with { floor = 25 };
        _validator.TestValidate(dto).ShouldHaveValidationErrorFor(x => x.floor);

        dto = dto with { floor = 5 };
        _validator.TestValidate(dto).ShouldNotHaveValidationErrorFor(x => x.floor);
    }

    [Test]
    public void Section_And_ShelfCode_Are_Required_And_Limited()
    {
        var dto = new ItemLocationDto { floor = 1, section = "", shelf_code = "" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.section);
        result.ShouldHaveValidationErrorFor(x => x.shelf_code);

        dto = dto with { section = new string('S', 11), shelf_code = new string('C', 21) };
        result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.section);
        result.ShouldHaveValidationErrorFor(x => x.shelf_code);

        dto = dto with { section = "Fiction", shelf_code = "F-123" };
        _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }
}
