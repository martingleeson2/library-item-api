using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application.Dtos
{
    public record ItemLocationDto
    {
        public required int floor { get; init; }
        public required string section { get; init; }
        public required string shelf_code { get; init; }
        public string? wing { get; init; }
        public string? position { get; init; }
        public string? notes { get; init; }
    }

    public record ItemDto
    {
        public required Guid id { get; init; }
        public required string title { get; init; }
        public string? subtitle { get; init; }
        public string? author { get; init; }
        public List<string>? contributors { get; init; }
        public string? isbn { get; init; }
        public string? issn { get; init; }
        public string? publisher { get; init; }
        public DateOnly? publication_date { get; init; }
        public string? edition { get; init; }
        public int? pages { get; init; }
        public string? language { get; init; }
        public required ItemType item_type { get; init; }
        public required string call_number { get; init; }
        public required ClassificationSystem classification_system { get; init; }
        public string? collection { get; init; }
        public required ItemLocationDto location { get; init; }
        public required ItemStatus status { get; init; }
        public string? barcode { get; init; }
        public DateOnly? acquisition_date { get; init; }
        public decimal? cost { get; init; }
        public string? condition_notes { get; init; }
        public string? description { get; init; }
        public List<string>? subjects { get; init; }
        public Uri? digital_url { get; init; }
        public required DateTime created_at { get; init; }
        public required DateTime updated_at { get; init; }
        public string? created_by { get; init; }
        public string? updated_by { get; init; }
        public Links? _links { get; init; }
    }
}