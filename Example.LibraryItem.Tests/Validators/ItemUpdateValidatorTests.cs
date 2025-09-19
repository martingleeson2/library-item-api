using Example.LibraryItem.Application.Validators;
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

    [Test]
    public void Optional_String_Fields_Skip_Validation_When_Null_Or_Empty()
    {
        var dto = Valid() with
        {
            Subtitle = null,
            Author = "",
            Isbn = null,
            Issn = "",
            Publisher = null,
            Edition = "",
            Language = null,
            Collection = "",
            Barcode = null,
            ConditionNotes = "",
            Description = null
        };

        var result = _validator.TestValidate(dto);
        
        // Should not have validation errors for null/empty optional fields
        result.ShouldNotHaveValidationErrorFor(x => x.Subtitle);
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
        result.ShouldNotHaveValidationErrorFor(x => x.Isbn);
        result.ShouldNotHaveValidationErrorFor(x => x.Issn);
        result.ShouldNotHaveValidationErrorFor(x => x.Publisher);
        result.ShouldNotHaveValidationErrorFor(x => x.Edition);
        result.ShouldNotHaveValidationErrorFor(x => x.Language);
        result.ShouldNotHaveValidationErrorFor(x => x.Collection);
        result.ShouldNotHaveValidationErrorFor(x => x.Barcode);
        result.ShouldNotHaveValidationErrorFor(x => x.ConditionNotes);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Optional_Numeric_Fields_Skip_Validation_When_Null()
    {
        var dto = Valid() with
        {
            Pages = null,
            Cost = null,
            PublicationDate = null,
            AcquisitionDate = null
        };

        var result = _validator.TestValidate(dto);
        
        // Should not have validation errors for null optional numeric fields
        result.ShouldNotHaveValidationErrorFor(x => x.Pages);
        result.ShouldNotHaveValidationErrorFor(x => x.Cost);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationDate);
        result.ShouldNotHaveValidationErrorFor(x => x.AcquisitionDate);
    }

    [Test]
    public void Optional_Collections_Skip_Validation_When_Null()
    {
        var dto = Valid() with
        {
            Contributors = null,
            Subjects = null,
            DigitalUrl = null
        };

        var result = _validator.TestValidate(dto);
        
        // Should not have validation errors for null optional collections
        result.ShouldNotHaveValidationErrorFor(x => x.Contributors);
        result.ShouldNotHaveValidationErrorFor(x => x.Subjects);
        result.ShouldNotHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void Isbn_And_Issn_Format_Validate_When_Present()
    {
        var dto = Valid() with 
        { 
            Isbn = "invalid-isbn",
            Issn = "invalid-issn"
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Isbn);
        result.ShouldHaveValidationErrorFor(x => x.Issn);
    }

    [Test]
    public void Numeric_Fields_Validate_When_Present()
    {
        var dto = Valid() with
        {
            Pages = 0, // Should fail - must be > 0
            Cost = -1 // Should fail - must be >= 0
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Pages);
        result.ShouldHaveValidationErrorFor(x => x.Cost);
    }

    [Test]
    public void Date_Fields_Validate_When_Present()
    {
        var dto = Valid() with
        {
            PublicationDate = DateOnly.FromDateTime(DateTime.Today.AddYears(15)), // Should fail - too far in future
            AcquisitionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) // Should fail - in future
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.PublicationDate);
        result.ShouldHaveValidationErrorFor(x => x.AcquisitionDate);
    }

    [Test]
    public void Collections_Validate_Size_When_Present()
    {
        var dto = Valid() with
        {
            Contributors = Enumerable.Repeat("contributor", 21).ToList(), // Should fail - max 20
            Subjects = Enumerable.Repeat("subject", 51).ToList() // Should fail - max 50
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Contributors);
        result.ShouldHaveValidationErrorFor(x => x.Subjects);
    }

    [Test]
    public void DigitalUrl_Validates_When_Present()
    {
        var dto = Valid() with
        {
            DigitalUrl = new Uri("ftp://invalid.com") // Should fail - must be HTTP(S)
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void Contributors_Collection_Validates_Size_Limit()
    {
        // Test the Must() validation branch for Contributors collection size
        var dto = Valid() with
        {
            Contributors = Enumerable.Range(1, 21).Select(i => $"Contributor {i}").ToList() // 21 > 20 limit
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Contributors)
            .WithErrorMessage("Cannot have more than 20 contributors");
    }

    [Test]
    public void Subjects_Collection_Validates_Size_Limit()
    {
        // Test the Must() validation branch for Subjects collection size
        var dto = Valid() with
        {
            Subjects = Enumerable.Range(1, 51).Select(i => $"Subject {i}").ToList() // 51 > 50 limit
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Subjects)
            .WithErrorMessage("Cannot have more than 50 subject tags");
    }

    [Test]
    public void DigitalUrl_Validates_Non_Absolute_Uri()
    {
        // Test the Must() validation branch for non-absolute URI
        var dto = Valid() with
        {
            DigitalUrl = new Uri("/relative/path", UriKind.Relative) // Not absolute
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.DigitalUrl)
            .WithErrorMessage("Digital URL must be a valid HTTP or HTTPS URL");
    }

    [Test]
    public void DigitalUrl_Validates_Invalid_Scheme()
    {
        // Test the Must() validation branch for invalid scheme
        var dto = Valid() with
        {
            DigitalUrl = new Uri("ftp://example.com") // Invalid scheme (not http/https)
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.DigitalUrl)
            .WithErrorMessage("Digital URL must be a valid HTTP or HTTPS URL");
    }

    [Test]
    public void DigitalUrl_WithValidHttpScheme_IsAccepted()
    {
        // Test that valid HTTP URLs pass validation
        var dto = Valid() with
        {
            DigitalUrl = new Uri("http://example.com/path")
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldNotHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void DigitalUrl_WithValidHttpsScheme_IsAccepted()
    {
        // Test that valid HTTPS URLs pass validation
        var dto = Valid() with
        {
            DigitalUrl = new Uri("https://example.com/path")
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldNotHaveValidationErrorFor(x => x.DigitalUrl);
    }
}
