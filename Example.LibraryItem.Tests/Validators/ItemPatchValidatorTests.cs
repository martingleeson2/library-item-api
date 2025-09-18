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

    [Test]
    public void Fields_Skip_Validation_When_Null()
    {
        var dto = new ItemPatchRequestDto
        {
            Title = null,
            Subtitle = null,
            Author = null,
            Isbn = null,
            Issn = null,
            Publisher = null,
            Edition = null,
            Language = null,
            Collection = null,
            CallNumber = null,
            Barcode = null,
            ConditionNotes = null,
            Description = null,
            ItemType = null,
            ClassificationSystem = null,
            Status = null,
            Location = null,
            Contributors = null,
            Subjects = null,
            DigitalUrl = null
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void String_Fields_Skip_Validation_When_Empty()
    {
        var dto = new ItemPatchRequestDto
        {
            Subtitle = "",
            Author = "",
            Isbn = "",
            Issn = "",
            Publisher = "",
            Edition = "",
            Language = "",
            Collection = "",
            Barcode = "",
            ConditionNotes = "",
            Description = ""
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Numeric_Fields_Skip_Validation_When_Null()
    {
        var dto = new ItemPatchRequestDto
        {
            Pages = null,
            Cost = null,
            PublicationDate = null,
            AcquisitionDate = null
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Numeric_Fields_Validate_When_Present()
    {
        var dto = new ItemPatchRequestDto
        {
            Pages = 0, // Should fail - must be > 0
            Cost = -1, // Should fail - must be >= 0
            PublicationDate = DateOnly.FromDateTime(DateTime.Today.AddYears(15)), // Should fail
            AcquisitionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) // Should fail
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Pages);
        result.ShouldHaveValidationErrorFor(x => x.Cost);
        result.ShouldHaveValidationErrorFor(x => x.PublicationDate);
        result.ShouldHaveValidationErrorFor(x => x.AcquisitionDate);
    }

    [Test]
    public void Enum_Fields_Skip_Validation_When_Null()
    {
        var dto = new ItemPatchRequestDto
        {
            ItemType = null,
            ClassificationSystem = null,
            Status = null
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Enum_Fields_Validate_When_Present()
    {
        var dto = new ItemPatchRequestDto
        {
            ItemType = (ItemType)999, // Invalid
            ClassificationSystem = (ClassificationSystem)999, // Invalid
            Status = (ItemStatus)999 // Invalid
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.ItemType);
        result.ShouldHaveValidationErrorFor(x => x.ClassificationSystem);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Test]
    public void Location_Validates_When_Present()
    {
        var dto = new ItemPatchRequestDto
        {
            Location = new ItemLocationDto { Floor = 1, Section = "", ShelfCode = "" } // Invalid
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Location.Section");
        result.ShouldHaveValidationErrorFor("Location.ShelfCode");
    }

    [Test]
    public void Collections_Skip_Validation_When_Null()
    {
        var dto = new ItemPatchRequestDto
        {
            Contributors = null,
            Subjects = null
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Collections_Validate_Size_When_Present()
    {
        var dto = new ItemPatchRequestDto
        {
            Contributors = Enumerable.Repeat("contributor", 21).ToList(), // Should fail - max 20
            Subjects = Enumerable.Repeat("subject", 51).ToList() // Should fail - max 50
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Contributors);
        result.ShouldHaveValidationErrorFor(x => x.Subjects);
    }

    [Test]
    public void DigitalUrl_Skips_Validation_When_Null()
    {
        var dto = new ItemPatchRequestDto
        {
            DigitalUrl = null
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void DigitalUrl_Validates_When_Present()
    {
        var dto = new ItemPatchRequestDto
        {
            DigitalUrl = new Uri("ftp://invalid.com") // Should fail - must be HTTP(S)
        };

        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void All_When_Conditions_False_Branches_Covered()
    {
        // Test that ensures ALL .When() conditions evaluate to false for ItemPatchRequestDto
        var dto = new ItemPatchRequestDto
        {
            // All fields are null to trigger the FALSE branch of .When() conditions
            Title = null,
            Subtitle = null,
            Author = null,
            Isbn = null,
            Issn = null,
            Publisher = null,
            Edition = null,
            Pages = null,
            Language = null,
            Collection = null,
            Barcode = null,
            Cost = null,
            ConditionNotes = null,
            Description = null,
            PublicationDate = null,
            AcquisitionDate = null,
            Status = null,
            Contributors = null,
            Subjects = null,
            DigitalUrl = null
        };

        var result = _validator.TestValidate(dto);
        
        // Should be valid since all optional fields are null
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void All_When_Conditions_True_Branches_With_Invalid_Values()
    {
        // Test that ensures ALL .When() conditions evaluate to true with invalid values
        var dto = new ItemPatchRequestDto
        {
            // All fields have invalid values to trigger validation errors
            Title = new string('x', 501), // Too long
            Subtitle = new string('x', 501), // Too long
            Author = new string('x', 256), // Too long
            Isbn = "invalid-isbn", // Invalid format
            Issn = "invalid-issn", // Invalid format
            Publisher = new string('x', 256), // Too long
            Edition = new string('x', 51), // Too long
            Pages = -1, // Invalid (must be > 0)
            Language = new string('x', 11), // Too long
            Collection = new string('x', 101), // Too long
            Barcode = new string('x', 51), // Too long
            Cost = -1, // Invalid (must be >= 0)
            ConditionNotes = new string('x', 1001), // Too long
            Description = new string('x', 2001), // Too long
            PublicationDate = DateOnly.FromDateTime(DateTime.Today.AddYears(15)), // Too far in future
            AcquisitionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)), // In future
            Status = (ItemStatus)999, // Invalid enum value
            Contributors = Enumerable.Repeat("Contributor", 21).ToList(), // Too many
            Subjects = Enumerable.Repeat("Subject", 51).ToList(), // Too many
            DigitalUrl = new Uri("ftp://invalid.com") // Invalid scheme
        };

        var result = _validator.TestValidate(dto);
        
        // Should have validation errors for all fields
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Subtitle);
        result.ShouldHaveValidationErrorFor(x => x.Author);
        result.ShouldHaveValidationErrorFor(x => x.Isbn);
        result.ShouldHaveValidationErrorFor(x => x.Issn);
        result.ShouldHaveValidationErrorFor(x => x.Publisher);
        result.ShouldHaveValidationErrorFor(x => x.Edition);
        result.ShouldHaveValidationErrorFor(x => x.Pages);
        result.ShouldHaveValidationErrorFor(x => x.Language);
        result.ShouldHaveValidationErrorFor(x => x.Collection);
        result.ShouldHaveValidationErrorFor(x => x.Barcode);
        result.ShouldHaveValidationErrorFor(x => x.Cost);
        result.ShouldHaveValidationErrorFor(x => x.ConditionNotes);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.PublicationDate);
        result.ShouldHaveValidationErrorFor(x => x.AcquisitionDate);
        result.ShouldHaveValidationErrorFor(x => x.Status);
        result.ShouldHaveValidationErrorFor(x => x.Contributors);
        result.ShouldHaveValidationErrorFor(x => x.Subjects);
        result.ShouldHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void Empty_String_Fields_Trigger_When_Condition_False_Branches()
    {
        // Test that empty strings trigger the FALSE branch (not just null)
        var dto = new ItemPatchRequestDto
        {
            // Note: Title uses .When(x => x.Title != null) so empty string still triggers validation
            // Other fields use .When(x => !string.IsNullOrEmpty(x.Field)) so empty string = false branch
            Subtitle = "",
            Author = "",
            Isbn = "",
            Issn = "",
            Publisher = "",
            Edition = "",
            Language = "",
            Collection = "",
            Barcode = "",
            ConditionNotes = "",
            Description = ""
        };

        var result = _validator.TestValidate(dto);
        
        // Should not have validation errors since empty strings trigger the false branch
        // (except Title which uses different .When() condition)
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
    public void Title_Empty_String_Triggers_Validation_Error()
    {
        // Title specifically uses .When(x => x.Title != null) so empty string triggers validation
        var dto = new ItemPatchRequestDto
        {
            Title = "" // Empty string is not null, so validation is triggered and fails on NotEmpty()
        };

        var result = _validator.TestValidate(dto);
        
        // Should have validation error since empty string triggers the .When() condition
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Test]
    public void Contributors_Collection_Validates_Size_Limit()
    {
        // Test the Must() validation branch for Contributors collection size
        var dto = new ItemPatchRequestDto
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
        var dto = new ItemPatchRequestDto
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
        var dto = new ItemPatchRequestDto
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
        var dto = new ItemPatchRequestDto
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
        var dto = new ItemPatchRequestDto
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
        var dto = new ItemPatchRequestDto
        {
            DigitalUrl = new Uri("https://example.com/path")
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldNotHaveValidationErrorFor(x => x.DigitalUrl);
    }

    [Test]
    public void Location_Validation_When_Provided()
    {
        // Test the Location validation branch when Location is provided in PATCH
        var dto = new ItemPatchRequestDto
        {
            Location = new ItemLocationDto 
            { 
                Floor = -3, // Invalid floor (must be between -2 and 20)
                Section = "", // Invalid section (required)
                ShelfCode = "B" 
            }
        };

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor("Location.Floor");
        result.ShouldHaveValidationErrorFor("Location.Section");
    }

    [Test]
    public void Valid_NonNull_Fields_Should_Pass()
    {
        var dto = new ItemPatchRequestDto
        {
            Title = "Valid Title",
            Subtitle = "Valid Subtitle",
            Author = "Valid Author",
            Isbn = "9780743273565",
            Issn = "0317-8471",
            Publisher = "Valid Publisher",
            Edition = "1st Edition",
            Pages = 100,
            Language = "en",
            Collection = "Valid Collection",
            CallNumber = "813.52 F553g",
            Barcode = "123456789",
            Cost = 10.99m,
            ConditionNotes = "Good condition",
            Description = "Valid description",
            ItemType = ItemType.book,
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            PublicationDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
            AcquisitionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            Status = ItemStatus.available,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            Contributors = new List<string> { "Contributor 1" },
            Subjects = new List<string> { "Subject 1" },
            DigitalUrl = new Uri("https://example.com")
        };

        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
