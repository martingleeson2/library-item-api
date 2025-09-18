using Example.LibraryItem.Api;
using Example.LibraryItem.Api.Extensions;
using Example.LibraryItem.Api.Interfaces;
using Example.LibraryItem.Api.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Example.LibraryItem.Tests.Api.Extensions;

/// <summary>
/// Tests for ServiceCollectionExtensions to ensure proper service registration.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Test]
    public void AddErrorHandling_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System); // Required dependency for ErrorResponseWriter

        // Act
        services.AddErrorHandling();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify all error handling services are registered
        var errorResponseWriter = serviceProvider.GetService<IErrorResponseWriter>();
        var requestContextService = serviceProvider.GetService<IRequestContextService>();
        var exceptionMappingService = serviceProvider.GetService<IExceptionMappingService>();

        errorResponseWriter.ShouldNotBeNull();
        requestContextService.ShouldNotBeNull();
        exceptionMappingService.ShouldNotBeNull();
        
        // Verify correct implementation types
        errorResponseWriter.ShouldBeOfType<ErrorResponseWriter>();
        requestContextService.ShouldBeOfType<RequestContextService>();
        exceptionMappingService.ShouldBeOfType<ExceptionMappingService>();
    }

    [Test]
    public void AddErrorHandling_RegistersServicesAsSingletons()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System); // Required dependency for ErrorResponseWriter

        // Act
        services.AddErrorHandling();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Verify singleton lifetime by getting services multiple times
        var errorResponseWriter1 = serviceProvider.GetService<IErrorResponseWriter>();
        var errorResponseWriter2 = serviceProvider.GetService<IErrorResponseWriter>();
        var requestContextService1 = serviceProvider.GetService<IRequestContextService>();
        var requestContextService2 = serviceProvider.GetService<IRequestContextService>();
        var exceptionMappingService1 = serviceProvider.GetService<IExceptionMappingService>();
        var exceptionMappingService2 = serviceProvider.GetService<IExceptionMappingService>();

        errorResponseWriter1.ShouldBeSameAs(errorResponseWriter2);
        requestContextService1.ShouldBeSameAs(requestContextService2);
        exceptionMappingService1.ShouldBeSameAs(exceptionMappingService2);
    }

    [Test]
    public void AddErrorHandling_ReturnsServiceCollection_ForMethodChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddErrorHandling();

        // Assert
        result.ShouldBeSameAs(services);
    }
}