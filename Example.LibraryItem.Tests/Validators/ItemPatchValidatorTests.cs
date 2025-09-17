using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Validators;

public class ItemPatchValidatorTests
{
    private ItemPatchValidator _validator = null!;

    [SetUp]
    public void SetUp()
    {
        _validator = new ItemPatchValidator();
    }

    [Test]
    public void All_Null_Fields_Should_Pass()
    {
        var dto = new ItemPatchRequestDto();
        _validator.TestValidate(dto).ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void NonNull_Fields_Are_Validated()
    {
        var dto = new ItemPatchRequestDto
        {
            Title = new string('a', 501),
            Isbn = "invalid",
            Issn = "invalid",
            Publisher = new string('p', 256),
            Edition = new string('e', 51),
            Language = new string('l', 11),
            Collection = new string('c', 101),
            Barcode = new string('b', 51),
            ConditionNotes = new string('n', 1001),
            Description = new string('d', 2001),
            Location = new ItemLocationDto { Floor = -2, Section = "", ShelfCode = "" }
        };

    var result = _validator.TestValidate(dto);
    result.ShouldHaveValidationErrorFor(x => x.Title);
    result.ShouldHaveValidationErrorFor(x => x.Isbn);
    result.ShouldHaveValidationErrorFor(x => x.Issn);
    result.ShouldHaveValidationErrorFor(x => x.Publisher);
    result.ShouldHaveValidationErrorFor(x => x.Edition);
    result.ShouldHaveValidationErrorFor(x => x.Language);
    result.ShouldHaveValidationErrorFor(x => x.Collection);
    result.ShouldHaveValidationErrorFor(x => x.Barcode);
    result.ShouldHaveValidationErrorFor(x => x.ConditionNotes);
    result.ShouldHaveValidationErrorFor(x => x.Description);
    // floor -2 is valid; only section/shelf_code should error
            result.ShouldHaveValidationErrorFor("Location.Section");
            result.ShouldHaveValidationErrorFor("Location.ShelfCode");
    }
}
