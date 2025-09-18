using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Example.LibraryItem.Api.Middleware;

namespace Example.LibraryItem.Tests.Api.Middleware;

/// <summary>
/// Unit tests for CorrelationIdMiddleware ensuring proper correlation ID handling,
/// request/response header management, and logging scope creation.
/// </summary>
[TestFixture]
public class CorrelationIdMiddlewareTests
{
    private Mock<RequestDelegate> _mockNext = null!;
    private Mock<ILogger<CorrelationIdMiddleware>> _mockLogger = null!;
    private CorrelationIdMiddleware _middleware = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        _middleware = new CorrelationIdMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
    }

    #region Valid GUID Extraction Tests

    /// <summary>
    /// Tests that a valid GUID in the X-Request-ID header is extracted and used correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithValidGuidInHeader_UsesProvidedGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(expectedGuid);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that a valid GUID in uppercase format is extracted correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithValidUppercaseGuidInHeader_UsesProvidedGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString().ToUpper();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(expectedGuid);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that a valid GUID with different formatting (with hyphens) is extracted correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithValidGuidWithHyphens_UsesProvidedGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString("D"); // Standard format with hyphens

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(expectedGuid);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Invalid GUID Handling Tests

    /// <summary>
    /// Tests that an invalid GUID format in the header results in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithInvalidGuidInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = "invalid-guid-123";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that an empty string in the header results in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithEmptyStringInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = "";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that whitespace-only content in the header results in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithWhitespaceInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = "   ";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that a partial GUID string results in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithPartialGuidInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = "12345678-1234-1234"; // Incomplete GUID

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Missing Header Tests

    /// <summary>
    /// Tests that when no X-Request-ID header is present, a new GUID is generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithMissingHeader_GeneratesNewGuid()
    {
        // Arrange
        // No header added to request

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that each request without a correlation ID gets a unique GUID.
    /// </summary>
    [Test]
    public async Task InvokeAsync_MultipleCallsWithoutHeader_GeneratesDifferentGuids()
    {
        // Arrange
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);

        // Assert
        var requestId1 = context1.Items[CorrelationIdMiddleware.RequestIdItemKey];
        var requestId2 = context2.Items[CorrelationIdMiddleware.RequestIdItemKey];
        
        requestId1.ShouldBeOfType<Guid>();
        requestId2.ShouldBeOfType<Guid>();
        ((Guid)requestId1!).ShouldNotBe((Guid)requestId2!);
        _mockNext.Verify(n => n(It.IsAny<HttpContext>()), Times.Exactly(2));
    }

    #endregion

    #region Multiple Header Values Tests

    /// <summary>
    /// Tests that when multiple X-Request-ID headers are present, only the first is used.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithMultipleHeaderValues_UsesFirstValidGuid()
    {
        // Arrange
        var firstGuid = Guid.NewGuid();
        var secondGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = new[] { firstGuid.ToString(), secondGuid.ToString() };

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(firstGuid);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(firstGuid.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that when multiple headers are present with the first being invalid, a new GUID is generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithMultipleHeaderValuesFirstInvalid_GeneratesNewGuid()
    {
        // Arrange
        var validGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = new[] { "invalid-guid", validGuid.ToString() };

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(validGuid); // Should not use the second valid GUID
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Response Header Tests

    /// <summary>
    /// Tests that the response always contains the X-Request-ID header with the processed GUID.
    /// </summary>
    [Test]
    public async Task InvokeAsync_Always_SetsResponseHeader()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Response.Headers.ContainsKey(CorrelationIdMiddleware.HeaderName).ShouldBeTrue();
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
    }

    /// <summary>
    /// Tests that the response header is set even when an exception occurs in the next middleware.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenNextMiddlewareThrows_StillSetsResponseHeader()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString();
        _mockNext.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext));
        exception.Message.ShouldBe("Test exception");
        
        // Response header should still be set
        _httpContext.Response.Headers.ContainsKey(CorrelationIdMiddleware.HeaderName).ShouldBeTrue();
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
    }

    #endregion

    #region HttpContext Items Tests

    /// <summary>
    /// Tests that the correlation ID is stored in HttpContext.Items for downstream middleware.
    /// </summary>
    [Test]
    public async Task InvokeAsync_Always_StoresRequestIdInHttpContextItems()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.ContainsKey(CorrelationIdMiddleware.RequestIdItemKey).ShouldBeTrue();
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(expectedGuid);
    }

    /// <summary>
    /// Tests that HttpContext.Items contains the generated GUID when no header is provided.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithoutHeader_StoresGeneratedGuidInHttpContextItems()
    {
        // Arrange
        // No header provided

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.ContainsKey(CorrelationIdMiddleware.RequestIdItemKey).ShouldBeTrue();
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
    }

    #endregion

    #region Logging Scope Tests

    /// <summary>
    /// Tests that a logging scope is created with the correlation ID.
    /// </summary>
    [Test]
    public async Task InvokeAsync_Always_CreatesLoggingScope()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            l => l.BeginScope(It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("RequestId") && d["RequestId"].Equals(expectedGuid))),
            Times.Once);
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that the logging scope is created even when a GUID is generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithGeneratedGuid_CreatesLoggingScopeWithGeneratedGuid()
    {
        // Arrange
        // No header provided, so GUID will be generated

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        _mockLogger.Verify(
            l => l.BeginScope(It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("RequestId") && d["RequestId"].Equals(requestId))),
            Times.Once);
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests that an empty GUID (all zeros) in the header results in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithEmptyGuidInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = Guid.Empty.ToString();

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldBe(Guid.Empty); // Guid.Empty is technically valid for Guid.TryParse
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(Guid.Empty.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that a GUID without hyphens is parsed correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithGuidWithoutHyphens_UsesProvidedGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = expectedGuid.ToString("N"); // No hyphens format

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey].ShouldBe(expectedGuid);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(expectedGuid.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that special characters in the header value result in a new GUID being generated.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithSpecialCharactersInHeader_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName] = "!@#$%^&*()_+";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        var requestId = _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey];
        requestId.ShouldBeOfType<Guid>();
        ((Guid)requestId!).ShouldNotBe(Guid.Empty);
        _httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString().ShouldBe(requestId.ToString());
        _mockNext.Verify(n => n(_httpContext), Times.Once);
    }

    #endregion

    #region Constants Tests

    /// <summary>
    /// Tests that the header name constant is correctly defined.
    /// </summary>
    [Test]
    public void HeaderName_ShouldBeXRequestId()
    {
        // Assert
        CorrelationIdMiddleware.HeaderName.ShouldBe("X-Request-ID");
    }

    /// <summary>
    /// Tests that the request ID item key constant is correctly defined.
    /// </summary>
    [Test]
    public void RequestIdItemKey_ShouldBeRequestId()
    {
        // Assert
        CorrelationIdMiddleware.RequestIdItemKey.ShouldBe("RequestId");
    }

    #endregion
}