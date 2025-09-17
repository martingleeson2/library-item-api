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
            title = new string('a', 501),
            isbn = "invalid",
            issn = "invalid",
            publisher = new string('p', 256),
            edition = new string('e', 51),
            language = new string('l', 11),
            collection = new string('c', 101),
            barcode = new string('b', 51),
            condition_notes = new string('n', 1001),
            description = new string('d', 2001),
            location = new ItemLocationDto { floor = -2, section = "", shelf_code = "" }
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.title);
        result.ShouldHaveValidationErrorFor(x => x.isbn);
        result.ShouldHaveValidationErrorFor(x => x.issn);
        result.ShouldHaveValidationErrorFor(x => x.publisher);
        result.ShouldHaveValidationErrorFor(x => x.edition);
        result.ShouldHaveValidationErrorFor(x => x.language);
        result.ShouldHaveValidationErrorFor(x => x.collection);
        result.ShouldHaveValidationErrorFor(x => x.barcode);
        result.ShouldHaveValidationErrorFor(x => x.condition_notes);
        result.ShouldHaveValidationErrorFor(x => x.description);
    // floor -2 is valid; only section/shelf_code should error
        result.ShouldHaveValidationErrorFor("location.section");
        result.ShouldHaveValidationErrorFor("location.shelf_code");
    }
}
