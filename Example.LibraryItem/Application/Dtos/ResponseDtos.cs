namespace Example.LibraryItem.Application.Dtos
{
    public record ItemListResponseDto
    {
        public required List<ItemDto> data { get; init; }
        public required PaginationDto pagination { get; init; }
        public Links? _links { get; init; }
    }

    public record ErrorResponseDto
    {
        public required string error { get; init; }
        public required string message { get; init; }
        public string? details { get; init; }
        public required DateTime timestamp { get; init; }
        public required Guid request_id { get; init; }
        public string? path { get; init; }
    }

    public record ValidationErrorResponseDto
    {
        public required string error { get; init; }
        public required string message { get; init; }
        public required List<ValidationError> validation_errors { get; init; }
        public required DateTime timestamp { get; init; }
        public required Guid request_id { get; init; }
    }
}