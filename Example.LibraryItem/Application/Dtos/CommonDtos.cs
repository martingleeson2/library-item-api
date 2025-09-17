namespace Example.LibraryItem.Application.Dtos
{
    public record Links
    {
        public Link? self { get; init; }
        public Link? next { get; init; }
        public Link? previous { get; init; }
        public Link? first { get; init; }
        public Link? last { get; init; }
    }

    public record Link
    {
        public required string href { get; init; }
    }

    public record PaginationDto
    {
        public required int page { get; init; }
        public required int limit { get; init; }
        public required int total_items { get; init; }
        public required int total_pages { get; init; }
        public required bool has_next { get; init; }
        public required bool has_previous { get; init; }
    }

    public record ValidationError
    {
        public required string field { get; init; }
        public required string message { get; init; }
    }
}