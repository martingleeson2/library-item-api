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

    public class ListItemsQueryValidator : AbstractValidator<ListItemsQuery>
    {
        public ListItemsQueryValidator()
        {
            RuleFor(x => x.page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.limit).InclusiveBetween(1, 100);
            RuleFor(x => x.sort_by).Must(v => v == null || new[] { "title", "author", "publication_date", "call_number", "created_at", "updated_at" }.Contains(v))
                .WithMessage("Invalid sort_by");
            RuleFor(x => x.sort_order).Must(v => v == null || v is "asc" or "desc").WithMessage("Invalid sort_order");
            RuleFor(x => x.publication_year_from).GreaterThanOrEqualTo(1000).When(x => x.publication_year_from.HasValue);
            RuleFor(x => x.publication_year_to).GreaterThanOrEqualTo(1000).When(x => x.publication_year_to.HasValue);
        }
    }
}
