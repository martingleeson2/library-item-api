using FluentValidation;
using Example.LibraryItem.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Example.LibraryItem.Application
{
    public record ListItemsQuery(
        [FromQuery(Name = "page")] int Page,
        [FromQuery(Name = "limit")] int Limit,
        [FromQuery(Name = "title")] string? Title,
        [FromQuery(Name = "author")] string? Author,
        [FromQuery(Name = "isbn")] string? Isbn,
        [FromQuery(Name = "item_type")] ItemType? ItemType,
        [FromQuery(Name = "status")] ItemStatus? Status,
        [FromQuery(Name = "collection")] string? Collection,
        [FromQuery(Name = "location_floor")] int? LocationFloor, 
        [FromQuery(Name = "location_section")] string? LocationSection, 
        [FromQuery(Name = "call_number")] string? CallNumber,
        [FromQuery(Name = "publication_year_from")] int? PublicationYearFrom, 
        [FromQuery(Name = "publication_year_to")] int? PublicationYearTo,
        [FromQuery(Name = "sort_by")] string? SortBy, 
        [FromQuery(Name = "sort_order")] string? SortOrder
    );

    /// <summary>
    /// Validator for ListItemsQuery following standardized DTO validation patterns
    /// </summary>
    public class ListItemsQueryValidator : AbstractValidator<ListItemsQuery>
    {
        // Centralize allowed sort fields to keep rules and messages in sync
        private static readonly string[] ValidSortFields = new[]
        {
            "title", "author", "publication_date", "call_number",
            "created_at", "updated_at", "item_type", "status"
        };

        public ListItemsQueryValidator()
        {
            // Pagination validation
            RuleFor(x => x.Page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be greater than or equal to 1");

            RuleFor(x => x.Limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100");

            // Search parameters validation
            RuleFor(x => x.Title)
                .MaximumLength(500)
                .WithMessage("Title filter cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Title));

            RuleFor(x => x.Author)
                .MaximumLength(255)
                .WithMessage("Author filter cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Author));

            RuleFor(x => x.Isbn)
                .Matches(@"^(?:978|979)?[0-9]{9}[0-9X]$")
                .WithMessage("ISBN format is invalid. Expected format: 9780743273565 or 978074327356X")
                .When(x => !string.IsNullOrEmpty(x.Isbn));

            RuleFor(x => x.Collection)
                .MaximumLength(100)
                .WithMessage("Collection filter cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Collection));

            RuleFor(x => x.CallNumber)
                .MaximumLength(50)
                .WithMessage("Call number filter cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.CallNumber));

            RuleFor(x => x.LocationSection)
                .MaximumLength(10)
                .WithMessage("Location section filter cannot exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.LocationSection));

            // Enum validations
            RuleFor(x => x.ItemType)
                .IsInEnum()
                .WithMessage("Invalid item type. Valid values are: book, periodical, dvd, cd, manuscript, digital_resource")
                .When(x => x.ItemType.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid status. Valid values are: available, checked_out, reserved, maintenance, lost, damaged")
                .When(x => x.Status.HasValue);

            // Location validation
            RuleFor(x => x.LocationFloor)
                .InclusiveBetween(-2, 20)
                .WithMessage("Floor must be between -2 and 20")
                .When(x => x.LocationFloor.HasValue);

            // Date range validation
            RuleFor(x => x.PublicationYearFrom)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("Publication year from must be 1000 or later")
                .When(x => x.PublicationYearFrom.HasValue);

            RuleFor(x => x.PublicationYearTo)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("Publication year to must be 1000 or later")
                .When(x => x.PublicationYearTo.HasValue);

            RuleFor(x => x)
                .Must(x => !x.PublicationYearFrom.HasValue || !x.PublicationYearTo.HasValue || x.PublicationYearFrom <= x.PublicationYearTo)
                .WithMessage("Publication year 'from' must be less than or equal to 'to'")
                .When(x => x.PublicationYearFrom.HasValue && x.PublicationYearTo.HasValue);

            // Sorting validation
            RuleFor(x => x.SortBy)
                .Must(BeValidSortField)
                .WithMessage(_ => $"Invalid sort field. Valid values are: {string.Join(", ", ValidSortFields)}")
                .When(x => !string.IsNullOrEmpty(x.SortBy));

            RuleFor(x => x.SortOrder)
                .Must(order => string.IsNullOrEmpty(order)
                    || order.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || order.Equals("desc", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Sort order must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.SortOrder));
        }

        /// <summary>
        /// Validates that the sort field is one of the allowed values
        /// </summary>
        private static bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return true;

            return Array.Exists(ValidSortFields, f => string.Equals(f, sortBy, StringComparison.OrdinalIgnoreCase));
        }
    }
}