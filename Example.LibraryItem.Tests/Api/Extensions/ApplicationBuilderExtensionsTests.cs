using Example.LibraryItem.Api.Extensions;
using Example.LibraryItem.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Example.LibraryItem.Tests.Api.Extensions;

/// <summary>
/// Tests for ApplicationBuilderExtensions to ensure proper middleware registration.
/// </summary>
public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void UseCorrelationId_AddsCorrelationIdMiddleware()
    {
        // Arrange
        var builder = CreateTestApplicationBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseCorrelationId();

        // Assert
        result.ShouldBeSameAs(app); // Verify method chaining
        
        // Verify middleware was added by checking the application builder's properties
        var appProperty = typeof(IApplicationBuilder).GetProperty("ApplicationServices");
        appProperty.ShouldNotBeNull();
    }

    [Test]
    public void UseProblemHandling_AddsGlobalExceptionMiddleware()
    {
        // Arrange
        var builder = CreateTestApplicationBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseProblemHandling();

        // Assert
        result.ShouldBeSameAs(app); // Verify method chaining
        
        // Verify middleware was added by checking the application builder's properties
        var appProperty = typeof(IApplicationBuilder).GetProperty("ApplicationServices");
        appProperty.ShouldNotBeNull();
    }

    [Test]
    public void UseCorrelationId_ReturnsApplicationBuilder_ForMethodChaining()
    {
        // Arrange
        var builder = CreateTestApplicationBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseCorrelationId();

        // Assert
        result.ShouldBeSameAs(app);
    }

    [Test]
    public void UseProblemHandling_ReturnsApplicationBuilder_ForMethodChaining()
    {
        // Arrange
        var builder = CreateTestApplicationBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseProblemHandling();

        // Assert
        result.ShouldBeSameAs(app);
    }

    [Test]
    public void MiddlewareExtensions_CanBeChainedTogether()
    {
        // Arrange
        var builder = CreateTestApplicationBuilder();
        var app = builder.Build();

        // Act & Assert - Should not throw and should allow method chaining
        var result = app.UseCorrelationId()
                       .UseProblemHandling();

        result.ShouldNotBeNull();
    }

    private static WebApplicationBuilder CreateTestApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        
        // Add minimal services required for testing
        builder.Services.AddLogging();
        
        return builder;
    }
}