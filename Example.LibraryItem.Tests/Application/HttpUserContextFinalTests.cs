using Example.LibraryItem.Application.Services;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Shouldly;
using System.Security.Claims;
using Moq;

namespace Example.LibraryItem.Tests.Application;

/// <summary>
/// Final tests for HttpUserContext to reach 90% branch coverage.
/// These tests specifically target the null HttpContext branch.
/// </summary>
[TestFixture]
public class HttpUserContextFinalTests
{
    [Test]
    public void CurrentUser_Returns_Null_When_HttpContext_Is_Null()
    {
        // Arrange - Create mock with null HttpContext to test the null branch
        var mockAccessor = new TestHttpContextAccessor
        {
            HttpContext = null // This triggers the null branch
        };
        var userContext = new HttpUserContext(mockAccessor);

        // Act
        var result = userContext.CurrentUser;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void CurrentUser_Returns_Null_When_User_Is_Null()
    {
        // Arrange - Create HttpContext with null User
        var mockContext = new Mock<HttpContext>();
        mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
        var mockAccessor = new TestHttpContextAccessor
        {
            HttpContext = mockContext.Object
        };
        var userContext = new HttpUserContext(mockAccessor);

        // Act
        var result = userContext.CurrentUser;

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public void CurrentUser_Returns_Null_When_Identity_Name_Is_Null()
    {
        // Arrange - Create HttpContext with User and Identity, but Name is null
        var identity = new System.Security.Claims.ClaimsIdentity();
        // Identity.Name is null by default if no name claim
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var context = new DefaultHttpContext
        {
            User = principal
        };
        var mockAccessor = new TestHttpContextAccessor
        {
            HttpContext = context
        };
        var userContext = new HttpUserContext(mockAccessor);

        // Act
        var result = userContext.CurrentUser;

        // Assert
        result.ShouldBeNull();
    }
}

/// <summary>
/// Test implementation of IHttpContextAccessor for testing null scenarios
/// </summary>
public class TestHttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }
}