using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Example.LibraryItem.Tests.Integration;

namespace Example.LibraryItem.Tests.Api.Authentication;

/// <summary>
/// Integration tests for ApiKeyAuthenticationHandler using WebApplicationFactory
/// to test the actual authentication behavior in a realistic HTTP context.
/// These tests ensure comprehensive coverage of all authentication scenarios.
/// </summary>
[TestFixture]
public class ApiKeyAuthenticationHandlerTests
{
    private AuthenticationTestFactory _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        var validKeys = new[] { "test-valid-key", "another-valid-key", "short" };
        _factory = new AuthenticationTestFactory(validKeys);
        _client = _factory.CreateUnauthenticatedClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    #region Successful Authentication Tests

    [Test]
    public async Task Request_WithValidApiKeyInHeader_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid-key");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Request_WithValidApiKeyInCustomHeader_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("another-valid-key", "X-API-Key");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Request_WithShortValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("short");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Missing API Key Tests

    [Test]
    public async Task Request_WithMissingApiKey_ReturnsUnauthorized()
    {
        // Arrange
        // Using unauthenticated client (no API key)

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithEmptyApiKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", "");

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithWhitespaceApiKeyHeader_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", "   ");

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Invalid API Key Tests

    [Test]
    public async Task Request_WithInvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("invalid-key-123");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithPartiallyMatchingApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid"); // Partial match

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithCaseSensitiveApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("TEST-VALID-KEY"); // Wrong case

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Configuration Tests

    [Test]
    public async Task Request_WithNoValidKeysConfigured_ReturnsUnauthorized()
    {
        // Arrange
        using var emptyKeysFactory = new AuthenticationTestFactory(new string[0]);
        var client = emptyKeysFactory.CreateClientWithApiKey("any-key");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region HTTP Methods Tests

    [Test]
    public async Task PostRequest_WithValidApiKey_ReturnsBadRequestForInvalidBody()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid-key");

        // Act - POST to an endpoint with null content (should cause model binding to fail)
        var response = await client.PostAsync("/v1/items", null);

        // Assert
        // Should authenticate successfully (not 401) but return 400 for invalid request body
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PostRequest_WithoutApiKey_ReturnsUnauthorized()
    {
        // Arrange
        // Using unauthenticated client

        // Act - POST to an endpoint that only accepts GET requests
        var response = await _client.PostAsync("/v1/items", null);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Multiple API Keys Tests

    [Test]
    public async Task Request_WithFirstValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid-key");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Request_WithSecondValidApiKey_ReturnsSuccess()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("another-valid-key");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    #endregion

    #region Bearer Token Tests (Should Fail)

    [Test]
    public async Task Request_WithBearerToken_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClientWithBearerToken("some-jwt-token");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task Request_WithVeryLongApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var longKey = new string('a', 1000) + "test-valid-key";
        var client = _factory.CreateClientWithApiKey(longKey);

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithSpecialCharactersInApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid-key!@#$%");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithNullApiKeyValue_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", (string?)null);

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Query Parameter Tests

    [Test]
    public async Task Request_WithValidApiKeyInQueryParameter_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/health?apikey=test-valid-key");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithInvalidApiKeyInQueryParameter_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/health?apikey=invalid-key");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithApiKeyInQueryParameterAndHeader_HeaderTakesPrecedence()
    {
        // Arrange
        var client = _factory.CreateClientWithApiKey("test-valid-key"); // Valid header
        
        // Act - Invalid query parameter should be ignored since header exists
        var response = await client.GetAsync("/health?apikey=invalid-key");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Request_WithEmptyApiKeyInQueryParameter_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/health?apikey=");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithQueryParameterButNoHeader_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateUnauthenticatedClient();

        // Act
        var response = await client.GetAsync("/health?apikey=another-valid-key");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Multiple Headers Tests

    [Test]
    public async Task Request_WithMultipleApiKeyHeaders_TreatsAsInvalid()
    {
        // Arrange
        // When multiple values are added to the same header, they get concatenated with commas
        // This results in a string like "test-valid-key,invalid-key" which is not a valid API key
        _client.DefaultRequestHeaders.Add("X-API-Key", new[] { "test-valid-key", "invalid-key" });

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        // The concatenated header value should be treated as invalid
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Request_WithMultipleInvalidApiKeyHeaders_ReturnsUnauthorized()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-API-Key", new[] { "invalid-key-1", "invalid-key-2" });

        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    #endregion
}