using System.Text.Json.Serialization;

namespace Example.LibraryItem.Application.Dtos
{
    public record ItemListResponseDto
    {
        [JsonPropertyName("data")] public required List<ItemDto> Data { get; init; }
        [JsonPropertyName("pagination")] public required PaginationDto Pagination { get; init; }
    }

    public record ItemResponseDto
    {
        [JsonPropertyName("data")] public required ItemDto Data { get; init; }
    }
}