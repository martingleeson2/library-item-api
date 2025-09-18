using Example.LibraryItem.Application;
using Example.LibraryItem.Domain;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace Example.LibraryItem.Tests.Validators;

public class ListItemsQueryValidatorTests
{
    private ListItemsQueryValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new ListItemsQueryValidator();
    }

    [Test]
    public void Page_Should_Be_Greater_Than_Zero()
    {
        var query = new ListItemsQuery(0, 10, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Page);

        query = query with { Page = 1 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Page);
    }

    [Test]
    public void Limit_Should_Be_Between_1_And_100()
    {
        var query = new ListItemsQuery(1, 0, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit);

        query = query with { Limit = 101 };
        result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.Limit);

        query = query with { Limit = 50 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.Limit);
    }

    [Test]
    public void SortBy_Should_Be_Valid_Field()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "invalid_field", null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SortBy);

        query = query with { SortBy = "title" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);

        query = query with { SortBy = "author" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);

        query = query with { SortBy = "publication_date" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);

        query = query with { SortBy = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortBy);
    }

    [Test]
    public void SortOrder_Should_Be_Asc_Or_Desc()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, null, "invalid");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.SortOrder);

        query = query with { SortOrder = "asc" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);

        query = query with { SortOrder = "desc" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);

        query = query with { SortOrder = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
    }

    [Test]
    public void PublicationYearFrom_Should_Be_Valid_When_Provided()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, 999, null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PublicationYearFrom);

        query = query with { PublicationYearFrom = 2000 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationYearFrom);

        query = query with { PublicationYearFrom = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationYearFrom);
    }

    [Test]
    public void PublicationYearTo_Should_Be_Valid_When_Provided()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, 999, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.PublicationYearTo);

        query = query with { PublicationYearTo = 2023 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationYearTo);

        query = query with { PublicationYearTo = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.PublicationYearTo);
    }

    [Test]
    public void Valid_Query_Should_Pass_Validation()
    {
        var query = new ListItemsQuery(
            Page: 1,
            Limit: 20,
            Title: "Test Book",
            Author: "Test Author",
            Isbn: "9780743273565",
            ItemType: ItemType.book,
            Status: ItemStatus.available,
            Collection: "Fiction",
            LocationFloor: 2,
            LocationSection: "Fiction",
            CallNumber: "001.42",
            PublicationYearFrom: 2000,
            PublicationYearTo: 2023,
            SortBy: "title",
            SortOrder: "asc"
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}