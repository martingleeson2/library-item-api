using Microsoft.AspNetCore.Http;
using Example.LibraryItem.Api.Services;
using FluentValidation;
using FluentValidation.Results;

namespace Example.LibraryItem.Tests.Api.Services;

/// <summary>
/// Unit tests for ExceptionMappingService ensuring proper mapping of .NET exceptions
/// to HTTP status codes and error information.
/// </summary>
[TestFixture]
public class ExceptionMappingServiceTests
{
    private ExceptionMappingService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new ExceptionMappingService();
    }

    #region ValidationException Tests

    /// <summary>
    /// Tests that ValidationException is mapped to 422 Unprocessable Entity.
    /// </summary>
    [Test]
    public void MapException_WithValidationException_Returns422()
    {
        // Arrange
        var validationError = new ValidationFailure("Property", "Error message");
        var exception = new ValidationException([validationError]);

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        errorCode.ShouldBe("VALIDATION_ERROR");
        message.ShouldBe("The request contains validation errors");
    }

    /// <summary>
    /// Tests that ValidationException with empty errors is still mapped correctly.
    /// </summary>
    [Test]
    public void MapException_WithEmptyValidationException_Returns422()
    {
        // Arrange
        var exception = new ValidationException([]);

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        errorCode.ShouldBe("VALIDATION_ERROR");
        message.ShouldBe("The request contains validation errors");
    }

    #endregion

    #region UnauthorizedAccessException Tests

    /// <summary>
    /// Tests that UnauthorizedAccessException is mapped to 403 Forbidden.
    /// </summary>
    [Test]
    public void MapException_WithUnauthorizedAccessException_Returns403()
    {
        // Arrange
        var exception = new UnauthorizedAccessException("Access denied");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status403Forbidden);
        errorCode.ShouldBe("FORBIDDEN");
        message.ShouldBe("Insufficient permissions");
    }

    /// <summary>
    /// Tests that UnauthorizedAccessException with empty message is mapped correctly.
    /// </summary>
    [Test]
    public void MapException_WithEmptyUnauthorizedAccessException_Returns403()
    {
        // Arrange
        var exception = new UnauthorizedAccessException();

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status403Forbidden);
        errorCode.ShouldBe("FORBIDDEN");
        message.ShouldBe("Insufficient permissions");
    }

    #endregion

    #region InvalidOperationException Tests

    /// <summary>
    /// Tests that InvalidOperationException is mapped to 409 Conflict with exception message.
    /// </summary>
    [Test]
    public void MapException_WithInvalidOperationException_Returns409WithMessage()
    {
        // Arrange
        var exceptionMessage = "The operation is invalid at this time";
        var exception = new InvalidOperationException(exceptionMessage);

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status409Conflict);
        errorCode.ShouldBe(exceptionMessage);
        message.ShouldBe(exceptionMessage);
    }

    /// <summary>
    /// Tests that InvalidOperationException with empty message is handled correctly.
    /// </summary>
    [Test]
    public void MapException_WithEmptyInvalidOperationException_Returns409WithEmptyMessage()
    {
        // Arrange
        var exception = new InvalidOperationException();

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status409Conflict);
        errorCode.ShouldBe(exception.Message);
        message.ShouldBe(exception.Message);
    }

    #endregion

    #region KeyNotFoundException Tests

    /// <summary>
    /// Tests that KeyNotFoundException is mapped to 404 Not Found.
    /// </summary>
    [Test]
    public void MapException_WithKeyNotFoundException_Returns404()
    {
        // Arrange
        var exception = new KeyNotFoundException("Resource not found");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status404NotFound);
        errorCode.ShouldBe("ITEM_NOT_FOUND");
        message.ShouldBe("The requested resource could not be found");
    }

    /// <summary>
    /// Tests that KeyNotFoundException with empty message is mapped correctly.
    /// </summary>
    [Test]
    public void MapException_WithEmptyKeyNotFoundException_Returns404()
    {
        // Arrange
        var exception = new KeyNotFoundException();

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status404NotFound);
        errorCode.ShouldBe("ITEM_NOT_FOUND");
        message.ShouldBe("The requested resource could not be found");
    }

    #endregion

    #region OperationCanceledException Tests

    /// <summary>
    /// Tests that OperationCanceledException is mapped to 499 Request Cancelled.
    /// </summary>
    [Test]
    public void MapException_WithOperationCanceledException_Returns499()
    {
        // Arrange
        var exception = new OperationCanceledException("Operation was cancelled");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(499);
        errorCode.ShouldBe("REQUEST_CANCELLED");
        message.ShouldBe("Request was cancelled");
    }

    /// <summary>
    /// Tests that OperationCanceledException with cancellation token is mapped correctly.
    /// </summary>
    [Test]
    public void MapException_WithOperationCancelledExceptionWithToken_Returns499()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var exception = new OperationCanceledException("Operation was cancelled", cts.Token);

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(499);
        errorCode.ShouldBe("REQUEST_CANCELLED");
        message.ShouldBe("Request was cancelled");
    }

    #endregion

    #region Generic Exception Tests

    /// <summary>
    /// Tests that unknown exceptions are mapped to 500 Internal Server Error.
    /// </summary>
    [Test]
    public void MapException_WithGenericException_Returns500()
    {
        // Arrange
        var exception = new ArgumentNullException("paramName", "Value cannot be null");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        errorCode.ShouldBe("INTERNAL_SERVER_ERROR");
        message.ShouldBe("An unexpected error occurred");
    }

    /// <summary>
    /// Tests that custom exceptions are mapped to 500 Internal Server Error.
    /// </summary>
    [Test]
    public void MapException_WithCustomException_Returns500()
    {
        // Arrange
        var exception = new CustomTestException("Custom error occurred");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        errorCode.ShouldBe("INTERNAL_SERVER_ERROR");
        message.ShouldBe("An unexpected error occurred");
    }

    /// <summary>
    /// Tests that system exceptions are mapped to 500 Internal Server Error.
    /// </summary>
    [Test]
    public void MapException_WithSystemException_Returns500()
    {
        // Arrange
        var exception = new OutOfMemoryException("Not enough memory");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        errorCode.ShouldBe("INTERNAL_SERVER_ERROR");
        message.ShouldBe("An unexpected error occurred");
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Tests that null exception is handled by the default case.
    /// </summary>
    [Test]
    public void MapException_WithNullException_ReturnsDefaultMapping()
    {
        // Arrange
        Exception? exception = null;

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception!);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        errorCode.ShouldBe("INTERNAL_SERVER_ERROR");
        message.ShouldBe("An unexpected error occurred");
    }

    /// <summary>
    /// Tests that inheritance hierarchy is respected (InvalidOperationException inherits from SystemException).
    /// </summary>
    [Test]
    public void MapException_WithInheritedException_UsesSpecificMapping()
    {
        // Arrange
        Exception exception = new InvalidOperationException("Specific invalid operation");

        // Act
        var (statusCode, errorCode, message) = _service.MapException(exception);

        // Assert
        statusCode.ShouldBe(StatusCodes.Status409Conflict);
        errorCode.ShouldBe("Specific invalid operation");
        message.ShouldBe("Specific invalid operation");
    }

    #endregion
}

/// <summary>
/// Custom exception class for testing purposes.
/// </summary>
public class CustomTestException : Exception
{
    public CustomTestException(string message) : base(message) { }
}