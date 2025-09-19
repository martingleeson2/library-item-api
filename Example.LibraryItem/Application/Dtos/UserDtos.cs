using System.Text.Json.Serialization;

namespace Example.LibraryItem.Application.Dtos
{
    public record UserInfoDto
    {
        [JsonPropertyName("user_id")] public required string UserId { get; init; }
        [JsonPropertyName("email")] public required string Email { get; init; }
        [JsonPropertyName("name")] public string? Name { get; init; }
        [JsonPropertyName("roles")] public required string[] Roles { get; init; }
    }

    public record LoginRequestDto
    {
        [JsonPropertyName("email")] public required string Email { get; init; }
        [JsonPropertyName("password")] public required string Password { get; init; }
    }

    public record LoginResponseDto
    {
        [JsonPropertyName("access_token")] public required string AccessToken { get; init; }
        [JsonPropertyName("token_type")] public required string TokenType { get; init; }
        [JsonPropertyName("expires_in")] public int ExpiresIn { get; init; }
        [JsonPropertyName("user")] public UserInfoDto? User { get; init; }
    }

    public record AuthenticationErrorDto
    {
        [JsonPropertyName("error")] public required string Error { get; init; }
        [JsonPropertyName("message")] public required string Message { get; init; }
        [JsonPropertyName("details")] public string? Details { get; init; }
    }
}
