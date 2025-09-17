using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application.Dtos
{
    public record ItemCreateRequestDto
    {
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
        public ItemStatus? status { get; init; }
        public string? barcode { get; init; }
        public DateOnly? acquisition_date { get; init; }
        public decimal? cost { get; init; }
        public string? condition_notes { get; init; }
        public string? description { get; init; }
        public List<string>? subjects { get; init; }
        public Uri? digital_url { get; init; }
    }

    public record ItemUpdateRequestDto
    {
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
    }

    public record ItemPatchRequestDto
    {
        public string? title { get; init; }
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
        public ItemType? item_type { get; init; }
        public string? call_number { get; init; }
        public ClassificationSystem? classification_system { get; init; }
        public string? collection { get; init; }
        public ItemLocationDto? location { get; init; }
        public ItemStatus? status { get; init; }
        public string? barcode { get; init; }
        public DateOnly? acquisition_date { get; init; }
        public decimal? cost { get; init; }
        public string? condition_notes { get; init; }
        public string? description { get; init; }
        public List<string>? subjects { get; init; }
        public Uri? digital_url { get; init; }
    }
}