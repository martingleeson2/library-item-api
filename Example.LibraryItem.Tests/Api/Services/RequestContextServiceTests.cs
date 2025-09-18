using Microsoft.AspNetCore.Http;
using Example.LibraryItem.Api.Services;
using Example.LibraryItem.Api.Middleware;

namespace Example.LibraryItem.Tests.Api.Services;

/// <summary>
/// Unit tests for RequestContextService ensuring proper request ID extraction
/// from HTTP context items and fallback behavior.
/// </summary>
[TestFixture]
public class RequestContextServiceTests
{
    private RequestContextService _service = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new RequestContextService();
        _httpContext = new DefaultHttpContext();
    }

    #region Valid GUID Tests

    /// <summary>
    /// Tests that a valid GUID in HttpContext.Items is returned correctly.
    /// </summary>
    [Test]
    public void GetRequestId_WithValidGuidInContext_ReturnsContextGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = expectedGuid;

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldBe(expectedGuid);
    }

    /// <summary>
    /// Tests that an empty GUID in HttpContext.Items is returned correctly.
    /// </summary>
    [Test]
    public void GetRequestId_WithEmptyGuidInContext_ReturnsEmptyGuid()
    {
        // Arrange
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = Guid.Empty;

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldBe(Guid.Empty);
    }

    #endregion

    #region Missing or Invalid Context Items Tests

    /// <summary>
    /// Tests that when no request ID is in context, a new GUID is generated.
    /// </summary>
    [Test]
    public void GetRequestId_WithMissingRequestId_GeneratesNewGuid()
    {
        // Arrange
        // No item added to context

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        result.ShouldBeOfType<Guid>();
    }

    /// <summary>
    /// Tests that when request ID is wrong type, a new GUID is generated.
    /// </summary>
    [Test]
    public void GetRequestId_WithWrongTypeInContext_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = "not-a-guid";

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        result.ShouldBeOfType<Guid>();
    }

    /// <summary>
    /// Tests that when request ID is null, a new GUID is generated.
    /// </summary>
    [Test]
    public void GetRequestId_WithNullInContext_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = null;

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        result.ShouldBeOfType<Guid>();
    }

    #endregion

    #region Multiple Calls Tests

    /// <summary>
    /// Tests that multiple calls with different contexts generate different GUIDs.
    /// </summary>
    [Test]
    public void GetRequestId_MultipleCallsWithoutContext_GeneratesDifferentGuids()
    {
        // Arrange
        var context1 = new DefaultHttpContext();
        var context2 = new DefaultHttpContext();

        // Act
        var result1 = _service.GetRequestId(context1);
        var result2 = _service.GetRequestId(context2);

        // Assert
        result1.ShouldNotBe(result2);
        result1.ShouldNotBe(Guid.Empty);
        result2.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Tests that multiple calls with the same context return the same GUID.
    /// </summary>
    [Test]
    public void GetRequestId_MultipleCallsWithSameContext_ReturnsSameGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = expectedGuid;

        // Act
        var result1 = _service.GetRequestId(_httpContext);
        var result2 = _service.GetRequestId(_httpContext);

        // Assert
        result1.ShouldBe(expectedGuid);
        result2.ShouldBe(expectedGuid);
        result1.ShouldBe(result2);
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests that when Items dictionary has the key but with boxed value, it works correctly.
    /// </summary>
    [Test]
    public void GetRequestId_WithBoxedGuidInContext_ReturnsGuid()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        object boxedGuid = expectedGuid;
        _httpContext.Items[CorrelationIdMiddleware.RequestIdItemKey] = boxedGuid;

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldBe(expectedGuid);
    }

    /// <summary>
    /// Tests that the service handles empty Items dictionary gracefully.
    /// </summary>
    [Test]
    public void GetRequestId_WithEmptyItemsDictionary_GeneratesNewGuid()
    {
        // Arrange
        _httpContext.Items.Clear();

        // Act
        var result = _service.GetRequestId(_httpContext);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        result.ShouldBeOfType<Guid>();
    }

    #endregion
}