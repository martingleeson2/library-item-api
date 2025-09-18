using System.Security.Claims;
using Example.LibraryItem.Application.Interfaces;
using Example.LibraryItem.Application.Services;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Shouldly;

namespace Example.LibraryItem.Tests.Application;

public class HttpUserContextTests
{
    [Test]
    public void Returns_Null_When_No_User()
    {
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBeNull();
    }

    [Test]
    public void Returns_Name_When_Identity_Present()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "alice") }, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBe("alice");
    }

    [Test]
    public void Returns_Null_When_HttpContext_Is_Null()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBeNull();
    }

    [Test]
    public void Returns_Null_When_User_Is_Null()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = null!;
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBeNull();
    }

    [Test]
    public void Returns_Null_When_Identity_Is_Null()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal();
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBeNull();
    }

    [Test]
    public void Returns_Null_When_Identity_Name_Is_Null()
    {
        var httpContext = new DefaultHttpContext();
        var identity = new ClaimsIdentity();
        httpContext.User = new ClaimsPrincipal(identity);
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var ctx = new HttpUserContext(accessor);
        ctx.CurrentUser.ShouldBeNull();
    }
}
 
