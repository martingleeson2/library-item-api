using System.Text.Json.Serialization;

namespace Example.LibraryItem.Application.Dtos
{
    public record PaginationDto
    {
        [JsonPropertyName("page")] public required int Page { get; init; }
        [JsonPropertyName("limit")] public required int Limit { get; init; }
        [JsonPropertyName("total_items")] public required int TotalItems { get; init; }
        [JsonPropertyName("total_pages")] public required int TotalPages { get; init; }
        [JsonPropertyName("has_next")] public required bool HasNext { get; init; }
        [JsonPropertyName("has_previous")] public required bool HasPrevious { get; init; }
    }
}