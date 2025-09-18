using System.Text.Json.Serialization;

namespace Example.LibraryItem.Application.Dtos
{
    public record ValidationError
    {
        [JsonPropertyName("field")] public required string Field { get; init; }
        [JsonPropertyName("message")] public required string Message { get; init; }
    }

    public record ErrorResponseDto
    {
        [JsonPropertyName("error")] public required string Error { get; init; }
        [JsonPropertyName("message")] public required string Message { get; init; }
        [JsonPropertyName("details")] public string? Details { get; init; }
        [JsonPropertyName("timestamp")] public required DateTime Timestamp { get; init; }
        [JsonPropertyName("request_id")] public required Guid RequestId { get; init; }
        [JsonPropertyName("path")] public string? Path { get; init; }
    }

    public record ValidationErrorResponseDto
    {
        [JsonPropertyName("error")] public required string Error { get; init; }
        [JsonPropertyName("message")] public required string Message { get; init; }
        [JsonPropertyName("validation_errors")] public required List<ValidationError> ValidationErrors { get; init; }
        [JsonPropertyName("timestamp")] public required DateTime Timestamp { get; init; }
        [JsonPropertyName("request_id")] public required Guid RequestId { get; init; }
    }
}