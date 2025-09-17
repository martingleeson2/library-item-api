using FluentValidation;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application
{
    public record ListItemsQuery(
        int page, int limit, string? title, string? author, string? isbn,
        ItemType? item_type, ItemStatus? status, string? collection,
        int? location_floor, string? location_section, string? call_number,
        int? publication_year_from, int? publication_year_to,
        string? sort_by, string? sort_order
    );

    /// <summary>
    /// Validator for ListItemsQuery following standardized DTO validation patterns
    /// </summary>
    public class ListItemsQueryValidator : AbstractValidator<ListItemsQuery>
    {
        public ListItemsQueryValidator()
        {
            // Pagination validation
            RuleFor(x => x.page)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Page must be greater than or equal to 1");

            RuleFor(x => x.limit)
                .InclusiveBetween(1, 100)
                .WithMessage("Limit must be between 1 and 100");

            // Search parameters validation
            RuleFor(x => x.title)
                .MaximumLength(500)
                .WithMessage("Title filter cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.title));

            RuleFor(x => x.author)
                .MaximumLength(255)
                .WithMessage("Author filter cannot exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.author));

            RuleFor(x => x.isbn)
                .Matches(@"^(?:978|979)?[0-9]{9}[0-9X]$")
                .WithMessage("ISBN format is invalid. Expected format: 9780743273565 or 978074327356X")
                .When(x => !string.IsNullOrEmpty(x.isbn));

            RuleFor(x => x.collection)
                .MaximumLength(100)
                .WithMessage("Collection filter cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.collection));

            RuleFor(x => x.call_number)
                .MaximumLength(50)
                .WithMessage("Call number filter cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.call_number));

            RuleFor(x => x.location_section)
                .MaximumLength(10)
                .WithMessage("Location section filter cannot exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.location_section));

            // Enum validations
            RuleFor(x => x.item_type)
                .IsInEnum()
                .WithMessage("Invalid item type. Valid values are: book, periodical, dvd, cd, manuscript, digital_resource")
                .When(x => x.item_type.HasValue);

            RuleFor(x => x.status)
                .IsInEnum()
                .WithMessage("Invalid status. Valid values are: available, checked_out, reserved, maintenance, lost, damaged")
                .When(x => x.status.HasValue);

            // Location validation
            RuleFor(x => x.location_floor)
                .InclusiveBetween(-2, 20)
                .WithMessage("Floor must be between -2 and 20")
                .When(x => x.location_floor.HasValue);

            // Date range validation
            RuleFor(x => x.publication_year_from)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("Publication year from must be 1000 or later")
                .When(x => x.publication_year_from.HasValue);

            RuleFor(x => x.publication_year_to)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("Publication year to must be 1000 or later")
                .When(x => x.publication_year_to.HasValue);

            RuleFor(x => x)
                .Must(x => !x.publication_year_from.HasValue || !x.publication_year_to.HasValue || x.publication_year_from <= x.publication_year_to)
                .WithMessage("Publication year 'from' must be less than or equal to 'to'")
                .When(x => x.publication_year_from.HasValue && x.publication_year_to.HasValue);

            // Sorting validation
            RuleFor(x => x.sort_by)
                .Must(BeValidSortField)
                .WithMessage("Invalid sort field. Valid values are: title, author, publication_date, call_number, created_at, updated_at")
                .When(x => !string.IsNullOrEmpty(x.sort_by));

            RuleFor(x => x.sort_order)
                .Must(order => order == null || order is "asc" or "desc")
                .WithMessage("Sort order must be 'asc' or 'desc'")
                .When(x => !string.IsNullOrEmpty(x.sort_order));
        }

        /// <summary>
        /// Validates that the sort field is one of the allowed values
        /// </summary>
        private static bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return true;

            var validSortFields = new[]
            {
                "title", "author", "publication_date", "call_number", 
                "created_at", "updated_at", "item_type", "status"
            };

            return validSortFields.Contains(sortBy.ToLowerInvariant());
        }
    }
}
