using Example.LibraryItem.Api.Services;
using Example.LibraryItem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Example.LibraryItem.Tests.Api.Services;

/// <summary>
/// Unit tests for <see cref="EndpointHelpers"/> focusing on CreateBadRequestResponse.
/// </summary>
[TestFixture]
public class EndpointHelpersTests
{
    private Mock<IDateTimeProvider> _dateTimeProviderMock = null!;
    private EndpointHelpers _helpers = null!;
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>(MockBehavior.Strict);
        _helpers = new EndpointHelpers(_dateTimeProviderMock.Object);
        _httpContext = new DefaultHttpContext();
        _httpContext.Request.Scheme = "https";
        _httpContext.Request.Host = new HostString("example.test");
        _httpContext.Request.Path = "/api/items";
    }

    /// <summary>
    /// Ensures CreateBadRequestResponse populates all fields correctly when details are provided.
    /// </summary>
    [Test]
    public void CreateBadRequestResponse_WithMessageAndDetails_SetsAllFields()
    {
        // Arrange
        var fixedUtc = new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedUtc);

        var message = "Invalid query parameter";
        var details = "Parameter 'sort' has an invalid value";

        // Act
        var result = _helpers.CreateBadRequestResponse(_httpContext, message, details);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBe("BAD_REQUEST");
        result.Message.ShouldBe(message);
        result.Details.ShouldBe(details);
        result.Timestamp.ShouldBe(fixedUtc);
        result.RequestId.ShouldNotBe(Guid.Empty);
        result.Path.ShouldBe(_httpContext.Request.Path.ToString());

        _dateTimeProviderMock.Verify(p => p.UtcNow, Times.Once);
        _dateTimeProviderMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Ensures CreateBadRequestResponse allows null details and still sets core fields.
    /// </summary>
    [Test]
    public void CreateBadRequestResponse_WithNullDetails_AllowsNullAndSetsCoreFields()
    {
        // Arrange
        var fixedUtc = new DateTime(2025, 05, 06, 07, 08, 09, DateTimeKind.Utc);
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(fixedUtc);

        var message = "Body payload is invalid";

        // Act
        var result = _helpers.CreateBadRequestResponse(_httpContext, message, null);

        // Assert
        result.ShouldNotBeNull();
        result.Error.ShouldBe("BAD_REQUEST");
        result.Message.ShouldBe(message);
        result.Details.ShouldBeNull();
        result.Timestamp.ShouldBe(fixedUtc);
        result.RequestId.ShouldNotBe(Guid.Empty);
        result.Path.ShouldBe(_httpContext.Request.Path.ToString());

        _dateTimeProviderMock.Verify(p => p.UtcNow, Times.Once);
        _dateTimeProviderMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Ensures GetCurrentUser returns the user name when identity is authenticated.
    /// </summary>
    [Test]
    public void GetCurrentUser_WithAuthenticatedUser_ReturnsUserName()
    {
        // Arrange
        var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test-user") }, "test");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        _httpContext.User = claimsPrincipal;

        // Act
        var result = _helpers.GetCurrentUser(_httpContext);

        // Assert
        result.ShouldBe("test-user");
    }

    /// <summary>
    /// Ensures GetCurrentUser returns null when user identity is null.
    /// </summary>
    [Test]
    public void GetCurrentUser_WithNullIdentity_ReturnsNull()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(); // No identity

        // Act
        var result = _helpers.GetCurrentUser(_httpContext);

        // Assert
        result.ShouldBeNull();
    }
}
