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

    [Test]
    public void Optional_Fields_Skip_Validation_When_Null_Or_Empty()
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
            AcquisitionDate = null,
            Status = null
        };

        var result = _validator.TestValidate(dto);
        
        // Should not have validation errors for null optional fields
        result.ShouldNotHaveValidationErrorFor(x => x.Pages);
        result.ShouldNotHaveValidationErrorFor(x => x.Cost);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationDate);
        result.ShouldNotHaveValidationErrorFor(x => x.AcquisitionDate);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
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
    public void Status_Validates_When_Present()
    {
        var dto = Valid() with { Status = (ItemStatus)999 }; // Invalid enum value

        var result = _validator.TestValidate(dto);
        
        result.ShouldHaveValidationErrorFor(x => x.Status);
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
    public void All_When_Conditions_False_Branches_Covered()
    {
        // Test that ensures ALL .When() conditions evaluate to false
        // This specifically tests the negative branches of all .When() conditions
        var dto = new ItemCreateRequestDto
        {
            Title = "Required Title",
            CallNumber = "REQ123",
            ItemType = ItemType.book,
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            
            // All these fields are null/empty to trigger the FALSE branch of .When() conditions
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
        
        // Should be valid since all optional fields are null/empty
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void All_When_Conditions_True_Branches_With_Invalid_Values()
    {
        // Test that ensures ALL .When() conditions evaluate to true with invalid values
        // This specifically tests the positive branches of all .When() conditions
        var dto = new ItemCreateRequestDto
        {
            Title = "Required Title",
            CallNumber = "REQ123", 
            ItemType = ItemType.book,
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            
            // All these fields have invalid values to trigger validation errors
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
        
        // Should have validation errors for all optional fields
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
        var dto = new ItemCreateRequestDto
        {
            Title = "Required Title",
            CallNumber = "REQ123",
            ItemType = ItemType.book,
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            
            // Empty strings should trigger FALSE branch of .When(x => !string.IsNullOrEmpty(x.Field))
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

    [Test]
    public void All_When_Conditions_True_Branches_With_Valid_Values()
    {
        // Test that ensures ALL .When() conditions evaluate to true with valid values
        // This specifically tests the positive branches of all .When() conditions with passing validations
        var dto = new ItemCreateRequestDto
        {
            Title = "Required Title",
            CallNumber = "REQ123",
            ItemType = ItemType.book,
            ClassificationSystem = ClassificationSystem.dewey_decimal,
            Location = new ItemLocationDto { Floor = 1, Section = "A", ShelfCode = "B" },
            
            // All these fields have valid values to trigger validation success
            Subtitle = "Valid Subtitle",
            Author = "Valid Author",
            Isbn = "9780743273565", // Valid ISBN
            Issn = "0317-8471", // Valid ISSN
            Publisher = "Valid Publisher",
            Edition = "1st Edition",
            Pages = 100, // Valid pages
            Language = "en", // Valid length
            Collection = "Valid Collection",
            Barcode = "123456789", // Valid length
            Cost = 10.99m, // Valid cost
            ConditionNotes = "Good condition",
            Description = "Valid description",
            PublicationDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), // Valid date
            AcquisitionDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), // Valid date
            Status = ItemStatus.available, // Valid enum
            Contributors = new List<string> { "Contributor 1" }, // Valid count
            Subjects = new List<string> { "Subject 1" }, // Valid count
            DigitalUrl = new Uri("https://example.com") // Valid URL
        };

        var result = _validator.TestValidate(dto);
        
        // Should be valid since all optional fields are valid
        result.ShouldNotHaveAnyValidationErrors();
    }
}
