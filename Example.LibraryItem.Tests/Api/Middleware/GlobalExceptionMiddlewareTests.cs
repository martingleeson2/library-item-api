using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Example.LibraryItem.Api.Middleware;
using Example.LibraryItem.Api.Interfaces;
using FluentValidation;
using FluentValidation.Results;

namespace Example.LibraryItem.Tests.Api.Middleware;

/// <summary>
/// Unit tests for GlobalExceptionMiddleware ensuring proper exception handling,
/// logging, error response generation, and service integration.
/// </summary>
[TestFixture]
public class GlobalExceptionMiddlewareTests
{
    private Mock<RequestDelegate> _mockNext = null!;
    private Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger = null!;
    private Mock<IErrorResponseWriter> _mockErrorWriter = null!;
    private Mock<IRequestContextService> _mockRequestContextService = null!;
    private Mock<IExceptionMappingService> _mockExceptionMappingService = null!;
    private GlobalExceptionMiddleware _middleware = null!;
    private DefaultHttpContext _httpContext = null!;
    private Guid _requestId;

    [SetUp]
    public void SetUp()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _mockErrorWriter = new Mock<IErrorResponseWriter>();
        _mockRequestContextService = new Mock<IRequestContextService>();
        _mockExceptionMappingService = new Mock<IExceptionMappingService>();
        
        _middleware = new GlobalExceptionMiddleware(
            _mockNext.Object,
            _mockLogger.Object,
            _mockErrorWriter.Object,
            _mockRequestContextService.Object,
            _mockExceptionMappingService.Object);
        
        _httpContext = new DefaultHttpContext();
        _requestId = Guid.NewGuid();
        
        // Setup default behavior for request context service
        _mockRequestContextService.Setup(s => s.GetRequestId(_httpContext))
            .Returns(_requestId);
    }

    #region Happy Path Tests

    /// <summary>
    /// Tests that when no exception occurs, the middleware calls the next delegate successfully.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenNoException_CallsNextSuccessfully()
    {
        // Arrange
        _mockNext.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(n => n(_httpContext), Times.Once);
        _mockRequestContextService.Verify(s => s.GetRequestId(_httpContext), Times.Once);
        _mockLogger.VerifyLogging(LogLevel.Information, Times.Never());
        _mockLogger.VerifyLogging(LogLevel.Warning, Times.Never());
        _mockLogger.VerifyLogging(LogLevel.Error, Times.Never());
    }

    #endregion

    #region ValidationException Tests

    /// <summary>
    /// Tests that ValidationException is handled correctly with appropriate logging and error response.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithValidationException_HandlesCorrectly()
    {
        // Arrange
        var validationError = new ValidationFailure("Property", "Error message");
        var validationException = new ValidationException([validationError]);
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockRequestContextService.Verify(s => s.GetRequestId(_httpContext), Times.Once);
        _mockLogger.VerifyLogging(LogLevel.Warning, Times.Once());
        _mockErrorWriter.Verify(w => w.WriteValidationErrorAsync(
            _httpContext, _requestId, validationException, _httpContext.RequestAborted), Times.Once);
        _mockExceptionMappingService.Verify(s => s.MapException(It.IsAny<Exception>()), Times.Never());
    }

    /// <summary>
    /// Tests that ValidationException logging includes the request ID.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithValidationException_LogsWithRequestId()
    {
        // Arrange
        var validationException = new ValidationException([]);
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Warning, 
            message => message.Contains("Validation failure") && message.Contains(_requestId.ToString()),
            Times.Once());
    }

    #endregion

    #region UnauthorizedAccessException Tests

    /// <summary>
    /// Tests that UnauthorizedAccessException is handled correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithUnauthorizedAccessException_HandlesCorrectly()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status403Forbidden, "FORBIDDEN", "Insufficient permissions"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Warning, Times.Once());
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(
            _httpContext, _requestId, StatusCodes.Status403Forbidden, "FORBIDDEN", "Insufficient permissions", _httpContext.RequestAborted), Times.Once);
    }

    #endregion

    #region InvalidOperationException Tests

    /// <summary>
    /// Tests that InvalidOperationException is handled correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithInvalidOperationException_HandlesCorrectly()
    {
        // Arrange
        var exception = new InvalidOperationException("Invalid operation");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status409Conflict, "Invalid operation", "Invalid operation"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Warning, Times.Once());
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(
            _httpContext, _requestId, StatusCodes.Status409Conflict, "Invalid operation", "Invalid operation", _httpContext.RequestAborted), Times.Once);
    }

    #endregion

    #region KeyNotFoundException Tests

    /// <summary>
    /// Tests that KeyNotFoundException is handled correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithKeyNotFoundException_HandlesCorrectly()
    {
        // Arrange
        var exception = new KeyNotFoundException("Resource not found");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status404NotFound, "ITEM_NOT_FOUND", "The requested resource could not be found"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Information, Times.Once());
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(
            _httpContext, _requestId, StatusCodes.Status404NotFound, "ITEM_NOT_FOUND", "The requested resource could not be found", _httpContext.RequestAborted), Times.Once);
    }

    #endregion

    #region OperationCanceledException Tests

    /// <summary>
    /// Tests that OperationCanceledException with cancelled token is handled correctly.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithCancelledOperationCanceledException_HandlesCorrectly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var exception = new OperationCanceledException("Operation was cancelled", cts.Token);
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Information, Times.Once());
        _httpContext.Response.StatusCode.ShouldBe(499);
        _mockErrorWriter.Verify(w => w.WriteValidationErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<ValidationException>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that OperationCanceledException without cancelled token is treated as regular exception.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithNonCancelledOperationCanceledException_TreatsAsRegularException()
    {
        // Arrange
        var exception = new OperationCanceledException("Operation was cancelled");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Error, Times.Once());
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(
            _httpContext, _requestId, StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred", _httpContext.RequestAborted), Times.Once);
    }

    #endregion

    #region Generic Exception Tests

    /// <summary>
    /// Tests that unknown exceptions are handled as internal server errors.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithGenericException_HandlesAsInternalServerError()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName", "Value cannot be null");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLogging(LogLevel.Error, Times.Once());
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(
            _httpContext, _requestId, StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred", _httpContext.RequestAborted), Times.Once);
    }

    #endregion

    #region Response Already Started Tests

    /// <summary>
    /// Tests that when response has already started, no error response is written.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_DoesNotWriteErrorResponse()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _httpContext.Response.StatusCode = 200;
        _httpContext.Response.Headers["Content-Type"] = "application/json";
        
        // Simulate response already started by creating a custom context
        var mockResponse = new Mock<HttpResponse>();
        mockResponse.Setup(r => r.HasStarted).Returns(true);
        
        var mockRequest = new Mock<HttpRequest>();
        mockRequest.Setup(r => r.Path).Returns("/api/test");
        mockRequest.Setup(r => r.Method).Returns("POST");
        
        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(c => c.Response).Returns(mockResponse.Object);
        mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
        mockHttpContext.Setup(c => c.RequestAborted).Returns(CancellationToken.None);
        
        _mockRequestContextService.Setup(s => s.GetRequestId(mockHttpContext.Object)).Returns(_requestId);
        _mockNext.Setup(n => n(mockHttpContext.Object)).ThrowsAsync(exception);

        // Act
        await _middleware.InvokeAsync(mockHttpContext.Object);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Warning, 
            message => message.Contains("Response already started") && message.Contains(_requestId.ToString()),
            Times.Once());
        _mockErrorWriter.Verify(w => w.WriteValidationErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<ValidationException>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockErrorWriter.Verify(w => w.WriteErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Error Writer Exception Tests

    /// <summary>
    /// Tests that when error writer throws an exception, it's logged but doesn't propagate.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenErrorWriterThrows_LogsButDoesNotPropagate()
    {
        // Arrange
        var originalException = new InvalidOperationException("Original exception");
        var writerException = new InvalidOperationException("Writer exception");
        
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(originalException);
        _mockExceptionMappingService.Setup(s => s.MapException(originalException))
            .Returns((StatusCodes.Status409Conflict, "CONFLICT", "Conflict occurred"));
        _mockErrorWriter.Setup(w => w.WriteErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(writerException);

        // Act & Assert (should not throw)
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Error, 
            message => message.Contains("Failed to write error response") && message.Contains(_requestId.ToString()),
            Times.Once());
    }

    #endregion

    #region Logging Scope Tests

    /// <summary>
    /// Tests that a logging scope is created with request context information.
    /// </summary>
    [Test]
    public async Task InvokeAsync_Always_CreatesLoggingScopeWithRequestContext()
    {
        // Arrange
        _httpContext.Request.Path = "/api/test";
        _httpContext.Request.Method = "POST";
        _mockNext.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockLogger.Verify(
            l => l.BeginScope(It.Is<Dictionary<string, object>>(d => 
                d.ContainsKey("RequestId") && d["RequestId"].Equals(_requestId) &&
                d.ContainsKey("Path") && d["Path"].Equals("/api/test") &&
                d.ContainsKey("Method") && d["Method"].Equals("POST"))),
            Times.Once);
    }

    #endregion

    #region Service Integration Tests

    /// <summary>
    /// Tests that request context service is called to get the request ID.
    /// </summary>
    [Test]
    public async Task InvokeAsync_Always_CallsRequestContextService()
    {
        // Arrange
        _mockNext.Setup(n => n(_httpContext)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockRequestContextService.Verify(s => s.GetRequestId(_httpContext), Times.Once);
    }

    /// <summary>
    /// Tests that exception mapping service is called for non-validation exceptions.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WithNonValidationException_CallsExceptionMappingService()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _mockNext.Setup(n => n(_httpContext)).ThrowsAsync(exception);
        _mockExceptionMappingService.Setup(s => s.MapException(exception))
            .Returns((StatusCodes.Status409Conflict, "CONFLICT", "Conflict occurred"));

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockExceptionMappingService.Verify(s => s.MapException(exception), Times.Once);
    }

    #endregion

    #region Cancellation Token Tests

    /// <summary>
    /// Tests that when cancellation is requested during error writing, the exception is not logged.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenCancellationDuringErrorWriting_DoesNotLogWriterException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var httpContext = new DefaultHttpContext();
        httpContext.RequestAborted = cts.Token;
        
        var originalException = new InvalidOperationException("Original exception");
        var writerException = new OperationCanceledException("Writer cancelled", cts.Token);
        
        _mockNext.Setup(n => n(httpContext)).ThrowsAsync(originalException);
        _mockRequestContextService.Setup(s => s.GetRequestId(httpContext)).Returns(_requestId);
        _mockExceptionMappingService.Setup(s => s.MapException(originalException))
            .Returns((StatusCodes.Status409Conflict, "CONFLICT", "Conflict occurred"));
        _mockErrorWriter.Setup(w => w.WriteErrorAsync(It.IsAny<HttpContext>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(writerException);

        cts.Cancel();

        // Act & Assert - The middleware should swallow the cancellation exception from the error writer
        await _middleware.InvokeAsync(httpContext);

        // Assert
        _mockLogger.VerifyLog(LogLevel.Error, 
            message => message.Contains("Failed to write error response"),
            Times.Never());
    }

    #endregion
}

/// <summary>
/// Extension methods for verifying logger interactions in tests.
/// </summary>
public static class LoggerTestExtensions
{
    /// <summary>
    /// Verifies that a log was written at the specified level a specific number of times.
    /// </summary>
    public static void VerifyLogging<T>(this Mock<ILogger<T>> mockLogger, LogLevel level, Times times)
    {
        mockLogger.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Verifies that a log was written at the specified level with a message matching the predicate.
    /// </summary>
    public static void VerifyLog<T>(this Mock<ILogger<T>> mockLogger, LogLevel level, Func<string, bool> messagePredicate, Times times)
    {
        mockLogger.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => messagePredicate(v.ToString() ?? "")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }
}