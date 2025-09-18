using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Example.LibraryItem.Api;

namespace Example.LibraryItem.Tests.Api;

[TestFixture]
public class ApiKeyValidatorTests
{
    private Mock<IWebHostEnvironment> _mockEnvironment = null!;
    private Mock<ILogger<Program>> _mockLogger = null!;
    private ServiceCollection _services = null!;
    private ServiceProvider _serviceProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<Program>>();
        _services = new ServiceCollection();
        _services.AddSingleton(_mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider?.Dispose();
    }

    [Test]
    public void ValidateOrCrash_WithNoApiKeys_ShouldThrowAndLogCritical()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>());
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("FATAL: No valid API keys configured in ApiKeys section");
        
        // Verify critical logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("FATAL: No valid API keys configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_WithEmptyApiKeysArray_ShouldNotThrow()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "" // Empty string - still counts as configured
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        // Empty string is considered a valid key by the configuration system
        Should.NotThrow(() => ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
    }

    [Test]
    public void ValidateOrCrash_InDevelopment_WithPlaceholderKeys_ShouldNotThrow()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "CHANGE-ME-OR-SERVER-WILL-CRASH",
            ["ApiKeys:1"] = "dev-key",
            ["ApiKeys:2"] = "valid-production-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act & Assert
        Should.NotThrow(() => ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Development environment") && v.ToString()!.Contains("3 key(s) configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_InTesting_WithPlaceholderKeys_ShouldNotThrow()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "test-key",
            ["ApiKeys:1"] = "REPLACE-THIS-KEY-BEFORE-PRODUCTION"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Testing");

        // Act & Assert
        Should.NotThrow(() => ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        // Verify information logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Testing environment") && v.ToString()!.Contains("2 key(s) configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_InProduction_WithPlaceholderKeys_ShouldThrowAndLogCritical()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "CHANGE-ME-OR-SERVER-WILL-CRASH",
            ["ApiKeys:1"] = "valid-production-key-12345"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("FATAL: Placeholder API keys detected in Production");
        exception.Message.ShouldContain("CHANGE-ME-OR-SERVER-WILL-CRASH");
        
        // Verify critical logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("FATAL: Placeholder API keys detected in Production")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_InStaging_WithPlaceholderKeys_ShouldThrowAndLogCritical()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "local-development-key",
            ["ApiKeys:1"] = "dev-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Staging");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("FATAL: Placeholder API keys detected in Staging");
        exception.Message.ShouldContain("local-development-key, dev-key");
        
        // Verify critical logging with multiple placeholder keys
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Critical,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("local-development-key, dev-key")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_InProduction_WithValidKeys_ShouldNotThrowAndLogSuccess()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "secure-production-key-abc123",
            ["ApiKeys:1"] = "another-secure-key-xyz789"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        Should.NotThrow(() => ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        // Verify success logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API key validation passed for Production") && v.ToString()!.Contains("2 key(s) configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_WithAllPlaceholderKeysInProduction_ShouldIncludeAllInErrorMessage()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "CHANGE-ME-OR-SERVER-WILL-CRASH",
            ["ApiKeys:1"] = "REPLACE-THIS-KEY-BEFORE-PRODUCTION",
            ["ApiKeys:2"] = "dev-key",
            ["ApiKeys:3"] = "test-key",
            ["ApiKeys:4"] = "local-development-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("CHANGE-ME-OR-SERVER-WILL-CRASH");
        exception.Message.ShouldContain("REPLACE-THIS-KEY-BEFORE-PRODUCTION");
        exception.Message.ShouldContain("dev-key");
        exception.Message.ShouldContain("test-key");
        exception.Message.ShouldContain("local-development-key");
    }

    [Test]
    public void ValidateOrCrash_WithMixedKeysInProduction_ShouldOnlyReportPlaceholderKeys()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "secure-production-key-1",
            ["ApiKeys:1"] = "CHANGE-ME-OR-SERVER-WILL-CRASH",
            ["ApiKeys:2"] = "another-secure-key-2",
            ["ApiKeys:3"] = "dev-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("CHANGE-ME-OR-SERVER-WILL-CRASH, dev-key");
        exception.Message.ShouldNotContain("secure-production-key");
    }

    [Test]
    public void ValidateOrCrash_WithCustomEnvironment_ShouldTreatAsProduction()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "test-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("CustomEnvironment");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("FATAL: Placeholder API keys detected in CustomEnvironment");
    }

    [Test]
    public void ValidateOrCrash_WithSingleValidKey_ShouldNotThrow()
    {
        // Arrange
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["ApiKeys:0"] = "single-secure-production-key"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        Should.NotThrow(() => ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        // Verify success logging with count
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("1 key(s) configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ValidateOrCrash_WithNullApiKeysSection_ShouldThrow()
    {
        // Arrange - Configuration without ApiKeys section
        var configuration = CreateConfiguration(new Dictionary<string, string>
        {
            ["SomeOtherSection:Key"] = "value"
        });
        _services.AddSingleton<IConfiguration>(configuration);
        _serviceProvider = _services.BuildServiceProvider();
        
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => 
            ApiKeyValidator.ValidateOrCrash(_serviceProvider, _mockEnvironment.Object));
        
        exception.Message.ShouldContain("FATAL: No valid API keys configured in ApiKeys section");
    }

    private static IConfiguration CreateConfiguration(Dictionary<string, string> configValues)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();
    }
}