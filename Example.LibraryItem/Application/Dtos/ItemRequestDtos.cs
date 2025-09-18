using System.Text.Json.Serialization;
using Example.LibraryItem.Domain;

namespace Example.LibraryItem.Application.Dtos
{
    public record ItemCreateRequestDto
    {
        [JsonPropertyName("title")] public required string Title { get; init; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; init; }
        [JsonPropertyName("author")] public string? Author { get; init; }
        [JsonPropertyName("contributors")] public List<string>? Contributors { get; init; }
        [JsonPropertyName("isbn")] public string? Isbn { get; init; }
        [JsonPropertyName("issn")] public string? Issn { get; init; }
        [JsonPropertyName("publisher")] public string? Publisher { get; init; }
        [JsonPropertyName("publication_date")] public DateOnly? PublicationDate { get; init; }
        [JsonPropertyName("edition")] public string? Edition { get; init; }
        [JsonPropertyName("pages")] public int? Pages { get; init; }
        [JsonPropertyName("language")] public string? Language { get; init; }
        [JsonPropertyName("item_type")] public required ItemType ItemType { get; init; }
        [JsonPropertyName("call_number")] public required string CallNumber { get; init; }
        [JsonPropertyName("classification_system")] public required ClassificationSystem ClassificationSystem { get; init; }
        [JsonPropertyName("collection")] public string? Collection { get; init; }
        [JsonPropertyName("location")] public required ItemLocationDto Location { get; init; }
        [JsonPropertyName("status")] public ItemStatus? Status { get; init; }
        [JsonPropertyName("barcode")] public string? Barcode { get; init; }
        [JsonPropertyName("acquisition_date")] public DateOnly? AcquisitionDate { get; init; }
        [JsonPropertyName("cost")] public decimal? Cost { get; init; }
        [JsonPropertyName("condition_notes")] public string? ConditionNotes { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("subjects")] public List<string>? Subjects { get; init; }
        [JsonPropertyName("digital_url")] public Uri? DigitalUrl { get; init; }
    }

    public record ItemUpdateRequestDto
    {
        [JsonPropertyName("title")] public required string Title { get; init; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; init; }
        [JsonPropertyName("author")] public string? Author { get; init; }
        [JsonPropertyName("contributors")] public List<string>? Contributors { get; init; }
        [JsonPropertyName("isbn")] public string? Isbn { get; init; }
        [JsonPropertyName("issn")] public string? Issn { get; init; }
        [JsonPropertyName("publisher")] public string? Publisher { get; init; }
        [JsonPropertyName("publication_date")] public DateOnly? PublicationDate { get; init; }
        [JsonPropertyName("edition")] public string? Edition { get; init; }
        [JsonPropertyName("pages")] public int? Pages { get; init; }
        [JsonPropertyName("language")] public string? Language { get; init; }
        [JsonPropertyName("item_type")] public required ItemType ItemType { get; init; }
        [JsonPropertyName("call_number")] public required string CallNumber { get; init; }
        [JsonPropertyName("classification_system")] public required ClassificationSystem ClassificationSystem { get; init; }
        [JsonPropertyName("collection")] public string? Collection { get; init; }
        [JsonPropertyName("location")] public required ItemLocationDto Location { get; init; }
        [JsonPropertyName("status")] public required ItemStatus Status { get; init; }
        [JsonPropertyName("barcode")] public string? Barcode { get; init; }
        [JsonPropertyName("acquisition_date")] public DateOnly? AcquisitionDate { get; init; }
        [JsonPropertyName("cost")] public decimal? Cost { get; init; }
        [JsonPropertyName("condition_notes")] public string? ConditionNotes { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("subjects")] public List<string>? Subjects { get; init; }
        [JsonPropertyName("digital_url")] public Uri? DigitalUrl { get; init; }
    }

    public record ItemPatchRequestDto
    {
        [JsonPropertyName("title")] public string? Title { get; init; }
        [JsonPropertyName("subtitle")] public string? Subtitle { get; init; }
        [JsonPropertyName("author")] public string? Author { get; init; }
        [JsonPropertyName("contributors")] public List<string>? Contributors { get; init; }
        [JsonPropertyName("isbn")] public string? Isbn { get; init; }
        [JsonPropertyName("issn")] public string? Issn { get; init; }
        [JsonPropertyName("publisher")] public string? Publisher { get; init; }
        [JsonPropertyName("publication_date")] public DateOnly? PublicationDate { get; init; }
        [JsonPropertyName("edition")] public string? Edition { get; init; }
        [JsonPropertyName("pages")] public int? Pages { get; init; }
        [JsonPropertyName("language")] public string? Language { get; init; }
        [JsonPropertyName("item_type")] public ItemType? ItemType { get; init; }
        [JsonPropertyName("call_number")] public string? CallNumber { get; init; }
        [JsonPropertyName("classification_system")] public ClassificationSystem? ClassificationSystem { get; init; }
        [JsonPropertyName("collection")] public string? Collection { get; init; }
        [JsonPropertyName("location")] public ItemLocationDto? Location { get; init; }
        [JsonPropertyName("status")] public ItemStatus? Status { get; init; }
        [JsonPropertyName("barcode")] public string? Barcode { get; init; }
        [JsonPropertyName("acquisition_date")] public DateOnly? AcquisitionDate { get; init; }
        [JsonPropertyName("cost")] public decimal? Cost { get; init; }
        [JsonPropertyName("condition_notes")] public string? ConditionNotes { get; init; }
        [JsonPropertyName("description")] public string? Description { get; init; }
        [JsonPropertyName("subjects")] public List<string>? Subjects { get; init; }
        [JsonPropertyName("digital_url")] public Uri? DigitalUrl { get; init; }
    }
}