using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Example.LibraryItem.Api.Services
{
    /// <summary>
    /// Service for generating and validating JWT tokens for library API authentication.
    /// Provides secure token-based authentication with configurable expiration and claims.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtConfiguration _jwtConfig;
        private readonly ILogger<JwtService> _logger;

        /// <summary>
        /// Initializes a new instance of the JWT service with configuration and logging.
        /// </summary>
        /// <param name="jwtConfig">JWT configuration containing secret key, issuer, and expiration settings</param>
        /// <param name="logger">Logger for authentication events and debugging</param>
        public JwtService(JwtConfiguration jwtConfig, ILogger<JwtService> logger)
        {
            _jwtConfig = jwtConfig ?? throw new ArgumentNullException(nameof(jwtConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a JWT token for the specified user with standard claims.
        /// Creates a token with user identity, email, and roles for library system access.
        /// </summary>
        /// <param name="userId">Unique identifier for the user (e.g., email or username)</param>
        /// <param name="userEmail">User's email address for identification</param>
        /// <param name="roles">User roles for authorization (e.g., "librarian", "admin", "user")</param>
        /// <returns>A signed JWT token string ready for Bearer authentication</returns>
        public string GenerateToken(string userId, string userEmail, IEnumerable<string> roles)
        {
            // Validate input parameters
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("User email cannot be null or empty", nameof(userEmail));

            _logger.LogInformation("Generating JWT token for user: {UserId}", userId);

            // Create JWT claims for user identity and authorization
            var claims = new List<Claim>
            {
                // Standard JWT claims
                new(JwtRegisteredClaimNames.Sub, userId),           // Subject (user identifier)
                new(JwtRegisteredClaimNames.Email, userEmail),      // Email address
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique identifier
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Issued at time
                
                // Custom claims for library system
                new("user_id", userId),
                new("email", userEmail)
            };

            // Add role claims for authorization
            foreach (var role in roles ?? Enumerable.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    claims.Add(new Claim("role", role)); // Additional role claim for flexibility
                }
            }

            // Create signing key from configured secret
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create JWT token with claims and expiration
            var token = new JwtSecurityToken(
                issuer: _jwtConfig.Issuer,
                audience: _jwtConfig.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtConfig.ExpirationInMinutes),
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            
            _logger.LogInformation("JWT token generated successfully for user: {UserId}, expires at: {ExpirationTime}", 
                userId, token.ValidTo);

            return tokenString;
        }

        /// <summary>
        /// Validates a JWT token and extracts user information.
        /// Verifies token signature, expiration, and extracts claims for authentication.
        /// </summary>
        /// <param name="token">The JWT token string to validate</param>
        /// <returns>ClaimsPrincipal containing user identity and claims, or null if invalid</returns>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Attempted to validate null or empty JWT token");
                return null;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtConfig.SecretKey);

                // Configure token validation parameters
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtConfig.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1) // Allow 1 minute clock skew
                };

                // Validate the token and extract claims
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                _logger.LogInformation("JWT token validated successfully for user: {UserId}", 
                    principal.FindFirst("user_id")?.Value ?? "unknown");

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("JWT token validation failed: Token has expired");
                return null;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("JWT token validation failed: Invalid signature");
                return null;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed: {ErrorMessage}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during JWT token validation");
                return null;
            }
        }

        /// <summary>
        /// Extracts the user ID from a JWT token without full validation.
        /// Useful for logging and debugging purposes.
        /// </summary>
        /// <param name="token">The JWT token string</param>
        /// <returns>User ID from token claims, or null if not found</returns>
        public string? GetUserIdFromToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(token);
                return jsonToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract user ID from JWT token");
                return null;
            }
        }
    }

    /// <summary>
    /// Interface for JWT token generation and validation services.
    /// Defines the contract for JWT operations in the library system.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a JWT token for user authentication.
        /// </summary>
        /// <param name="userId">Unique user identifier</param>
        /// <param name="userEmail">User's email address</param>
        /// <param name="roles">User roles for authorization</param>
        /// <returns>Signed JWT token string</returns>
        string GenerateToken(string userId, string userEmail, IEnumerable<string> roles);

        /// <summary>
        /// Validates a JWT token and returns user claims.
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>ClaimsPrincipal with user identity, or null if invalid</returns>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Extracts user ID from JWT token for logging/debugging.
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>User ID or null if not found</returns>
        string? GetUserIdFromToken(string token);
    }

    /// <summary>
    /// Configuration settings for JWT token generation and validation.
    /// Contains security parameters and token lifetime settings.
    /// </summary>
    public class JwtConfiguration
    {
        /// <summary>
        /// Secret key for signing JWT tokens. Should be at least 256 bits (32 characters) for security.
        /// In production, store this securely (e.g., Azure Key Vault, environment variables).
        /// </summary>
        public required string SecretKey { get; init; }

        /// <summary>
        /// Token issuer identifier. Usually the application or service name.
        /// Example: "library-management-api"
        /// </summary>
        public required string Issuer { get; init; }

        /// <summary>
        /// Token audience identifier. Usually the intended consumer of the token.
        /// Example: "library-users" or "library-client-app"
        /// </summary>
        public required string Audience { get; init; }

        /// <summary>
        /// Token expiration time in minutes. Default is 60 minutes (1 hour).
        /// Consider shorter times for high-security environments.
        /// </summary>
        public int ExpirationInMinutes { get; init; } = 60;
    }
}