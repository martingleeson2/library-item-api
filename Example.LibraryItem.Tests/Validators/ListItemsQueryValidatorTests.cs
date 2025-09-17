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
        result.ShouldHaveValidationErrorFor(x => x.page);

        query = query with { page = 1 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.page);
    }

    [Test]
    public void Limit_Should_Be_Between_1_And_100()
    {
        var query = new ListItemsQuery(1, 0, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.limit);

        query = query with { limit = 101 };
        result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.limit);

        query = query with { limit = 50 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.limit);
    }

    [Test]
    public void SortBy_Should_Be_Valid_Field()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, "invalid_field", null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.sort_by);

        query = query with { sort_by = "title" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_by);

        query = query with { sort_by = "author" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_by);

        query = query with { sort_by = "publication_date" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_by);

        query = query with { sort_by = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_by);
    }

    [Test]
    public void SortOrder_Should_Be_Asc_Or_Desc()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, null, null, "invalid");
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.sort_order);

        query = query with { sort_order = "asc" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_order);

        query = query with { sort_order = "desc" };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_order);

        query = query with { sort_order = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.sort_order);
    }

    [Test]
    public void PublicationYearFrom_Should_Be_Valid_When_Provided()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, 999, null, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.publication_year_from);

        query = query with { publication_year_from = 2000 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.publication_year_from);

        query = query with { publication_year_from = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.publication_year_from);
    }

    [Test]
    public void PublicationYearTo_Should_Be_Valid_When_Provided()
    {
        var query = new ListItemsQuery(1, 10, null, null, null, null, null, null, null, null, null, null, 999, null, null);
        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.publication_year_to);

        query = query with { publication_year_to = 2023 };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.publication_year_to);

        query = query with { publication_year_to = null };
        result = _validator.TestValidate(query);
        result.ShouldNotHaveValidationErrorFor(x => x.publication_year_to);
    }

    [Test]
    public void Valid_Query_Should_Pass_Validation()
    {
        var query = new ListItemsQuery(
            page: 1,
            limit: 20,
            title: "Test Book",
            author: "Test Author",
            isbn: "9780743273565",
            item_type: ItemType.book,
            status: ItemStatus.available,
            collection: "Fiction",
            location_floor: 2,
            location_section: "Fiction",
            call_number: "001.42",
            publication_year_from: 2000,
            publication_year_to: 2023,
            sort_by: "title",
            sort_order: "asc"
        );

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}