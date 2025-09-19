using Example.LibraryItem.Api.Services;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Example.LibraryItem.Tests.Api.Services;

/// <summary>
/// Unit tests for JwtService to ensure comprehensive coverage of JWT token operations.
/// Tests token generation, validation, and extraction functionality.
/// </summary>
[TestFixture]
public class JwtServiceTests
{
    private JwtConfiguration _jwtConfig = null!;
    private ILogger<JwtService> _logger = null!;
    private JwtService _jwtService = null!;

    [SetUp]
    public void Setup()
    {
        _jwtConfig = new JwtConfiguration
        {
            SecretKey = "TestSecretKeyForJwtServiceTesting12345678901234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60
        };

        _logger = new LoggerFactory().CreateLogger<JwtService>();
        _jwtService = new JwtService(_jwtConfig, _logger);
    }

    #region GenerateToken Tests

    [Test]
    public void GenerateToken_ValidParameters_ReturnsValidJwtToken()
    {
        // Arrange
        var userId = "test-user-123";
        var userEmail = "test@example.com";
        var roles = new[] { "user", "librarian" };

        // Act
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert
        token.ShouldNotBeNull();
        token.ShouldNotBeEmpty();

        // Verify token structure (should contain dots for header.payload.signature)
        var parts = token.Split('.');
        parts.Length.ShouldBe(3, "JWT token should have 3 parts separated by dots");
    }

    [Test]
    public void GenerateToken_IncludesCorrectClaims()
    {
        // Arrange
        var userId = "test-user-123";
        var userEmail = "test@example.com";
        var roles = new[] { "admin", "user" };

        // Act
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert - Decode and verify claims
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        jwtToken.Subject.ShouldBe(userId);
        jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value.ShouldBe(userEmail);
        jwtToken.Claims.First(c => c.Type == "user_id").Value.ShouldBe(userId);
        jwtToken.Claims.First(c => c.Type == "email").Value.ShouldBe(userEmail);

        // Check roles
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Count.ShouldBe(2);
        roleClaims.Select(c => c.Value).ShouldContain("admin");
        roleClaims.Select(c => c.Value).ShouldContain("user");
    }

    [Test]
    public void GenerateToken_IncludesUniqueJti()
    {
        // Arrange
        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };

        // Act
        var token1 = _jwtService.GenerateToken(userId, userEmail, roles);
        var token2 = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken1 = tokenHandler.ReadJwtToken(token1);
        var jwtToken2 = tokenHandler.ReadJwtToken(token2);

        jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value.ShouldNotBe(
            jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value
        );
    }

    [Test]
    public void GenerateToken_SetsCorrectExpiration()
    {
        // Arrange
        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var expectedExpiration = beforeGeneration.AddMinutes(_jwtConfig.ExpirationInMinutes);
        var actualExpiration = jwtToken.ValidTo;

        // Allow for small time differences
        var timeDifference = Math.Abs((expectedExpiration - actualExpiration).TotalSeconds);
        timeDifference.ShouldBeLessThan(5, "Token expiration should be within 5 seconds of expected time");
    }

    [Test]
    public void GenerateToken_NullUserId_ThrowsArgumentException()
    {
        // Arrange
        string userId = null!;
        var userEmail = "test@example.com";
        var roles = new[] { "user" };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            _jwtService.GenerateToken(userId, userEmail, roles));

        exception.Message.ShouldContain("User ID cannot be null or empty");
        exception.ParamName.ShouldBe("userId");
    }

    [Test]
    public void GenerateToken_EmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            _jwtService.GenerateToken(userId, userEmail, roles));

        exception.Message.ShouldContain("User ID cannot be null or empty");
    }

    [Test]
    public void GenerateToken_WhitespaceUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "   ";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            _jwtService.GenerateToken(userId, userEmail, roles));

        exception.Message.ShouldContain("User ID cannot be null or empty");
    }

    [Test]
    public void GenerateToken_NullUserEmail_ThrowsArgumentException()
    {
        // Arrange
        var userId = "test-user";
        string userEmail = null!;
        var roles = new[] { "user" };

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() =>
            _jwtService.GenerateToken(userId, userEmail, roles));

        exception.Message.ShouldContain("User email cannot be null or empty");
        exception.ParamName.ShouldBe("userEmail");
    }

    [Test]
    public void GenerateToken_EmptyRolesArray_WorksCorrectly()
    {
        // Arrange
        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = Array.Empty<string>();

        // Act
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert
        token.ShouldNotBeNull();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Should not have role claims
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Count.ShouldBe(0);
    }

    [Test]
    public void GenerateToken_NullRoles_WorksCorrectly()
    {
        // Arrange
        var userId = "test-user";
        var userEmail = "test@example.com";
        IEnumerable<string> roles = null!;

        // Act
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Assert
        token.ShouldNotBeNull();
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Should not have role claims
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Count.ShouldBe(0);
    }

    #endregion

    #region ValidateToken Tests

    [Test]
    public void ValidateToken_ValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = _jwtService.GenerateToken(userId, userEmail, roles);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.ShouldNotBeNull();
        principal.FindFirst("user_id")?.Value.ShouldBe(userId);
        principal.FindFirst("email")?.Value.ShouldBe(userEmail);
        principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value.ShouldBe(userId);
        principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value.ShouldBe(userEmail);
    }

    [Test]
    public void ValidateToken_InvalidSignature_ReturnsNull()
    {
        // Arrange - Create token with different secret
        var differentConfig = new JwtConfiguration
        {
            SecretKey = "DifferentSecretKeyForTesting12345678901234567890",
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            ExpirationInMinutes = 60
        };
        var differentService = new JwtService(differentConfig, _logger);

        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = differentService.GenerateToken(userId, userEmail, roles);

        // Act - Try to validate with different secret
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_ExpiredToken_ReturnsNull()
    {
        // Arrange - Create token that expires immediately
        var expiredConfig = new JwtConfiguration
        {
            SecretKey = _jwtConfig.SecretKey,
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            ExpirationInMinutes = -1 // Already expired
        };
        var expiredService = new JwtService(expiredConfig, _logger);

        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = expiredService.GenerateToken(userId, userEmail, roles);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_WrongIssuer_ReturnsNull()
    {
        // Arrange - Create token with different issuer
        var differentConfig = new JwtConfiguration
        {
            SecretKey = _jwtConfig.SecretKey,
            Issuer = "wrong-issuer",
            Audience = _jwtConfig.Audience,
            ExpirationInMinutes = 60
        };
        var differentService = new JwtService(differentConfig, _logger);

        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = differentService.GenerateToken(userId, userEmail, roles);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_WrongAudience_ReturnsNull()
    {
        // Arrange - Create token with different audience
        var differentConfig = new JwtConfiguration
        {
            SecretKey = _jwtConfig.SecretKey,
            Issuer = _jwtConfig.Issuer,
            Audience = "wrong-audience",
            ExpirationInMinutes = 60
        };
        var differentService = new JwtService(differentConfig, _logger);

        var userId = "test-user";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = differentService.GenerateToken(userId, userEmail, roles);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_NullToken_ReturnsNull()
    {
        // Act
        var principal = _jwtService.ValidateToken(null!);

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_EmptyToken_ReturnsNull()
    {
        // Act
        var principal = _jwtService.ValidateToken("");

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_WhitespaceToken_ReturnsNull()
    {
        // Act
        var principal = _jwtService.ValidateToken("   ");

        // Assert
        principal.ShouldBeNull();
    }

    [Test]
    public void ValidateToken_MalformedToken_ReturnsNull()
    {
        // Act
        var principal = _jwtService.ValidateToken("not-a-jwt-token");

        // Assert
        principal.ShouldBeNull();
    }

    #endregion

    #region GetUserIdFromToken Tests

    [Test]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = "test-user-123";
        var userEmail = "test@example.com";
        var roles = new[] { "user" };
        var token = _jwtService.GenerateToken(expectedUserId, userEmail, roles);

        // Act
        var userId = _jwtService.GetUserIdFromToken(token);

        // Assert
        userId.ShouldBe(expectedUserId);
    }

    [Test]
    public void GetUserIdFromToken_TokenWithoutUserIdClaim_ReturnsNull()
    {
        // Arrange - Create a token manually without user_id claim
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = System.Text.Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);
        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
                new Claim(JwtRegisteredClaimNames.Email, "test@example.com")
                // Note: No user_id claim
            }),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _jwtConfig.Issuer,
            Audience = _jwtConfig.Audience,
            SigningCredentials = credentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        // Act
        var userId = _jwtService.GetUserIdFromToken(tokenString);

        // Assert
        userId.ShouldBeNull();
    }

    [Test]
    public void GetUserIdFromToken_NullToken_ReturnsNull()
    {
        // Act
        var userId = _jwtService.GetUserIdFromToken(null!);

        // Assert
        userId.ShouldBeNull();
    }

    [Test]
    public void GetUserIdFromToken_EmptyToken_ReturnsNull()
    {
        // Act
        var userId = _jwtService.GetUserIdFromToken("");

        // Assert
        userId.ShouldBeNull();
    }

    [Test]
    public void GetUserIdFromToken_WhitespaceToken_ReturnsNull()
    {
        // Act
        var userId = _jwtService.GetUserIdFromToken("   ");

        // Assert
        userId.ShouldBeNull();
    }

    [Test]
    public void GetUserIdFromToken_MalformedToken_ReturnsNull()
    {
        // Act
        var userId = _jwtService.GetUserIdFromToken("not-a-jwt-token");

        // Assert
        userId.ShouldBeNull();
    }

    #endregion
}