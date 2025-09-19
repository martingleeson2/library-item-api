using Example.LibraryItem.Application.Dtos;

namespace Example.LibraryItem.Api.Services
{
    /// <summary>
    /// User authentication and management service for the library system.
    /// Provides user validation, role management, and authentication logic.
    /// 
    /// NOTE: This is a demo implementation with hardcoded users for development.
    /// In production, integrate with your actual user store (database, Active Directory, etc.).
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;

        /// <summary>
        /// Demo users for development and testing purposes.
        /// In production, replace with actual user store integration.
        /// </summary>
        private static readonly List<DemoUser> DemoUsers = new()
        {
            new DemoUser
            {
                UserId = "librarian@example.com",
                Email = "librarian@example.com",
                Password = "password123", // In production: use proper password hashing
                Name = "John Librarian",
                Roles = new[] { "librarian", "user" }
            },
            new DemoUser
            {
                UserId = "admin@example.com",
                Email = "admin@example.com",
                Password = "admin123",
                Name = "Jane Admin",
                Roles = new[] { "admin", "librarian", "user" }
            },
            new DemoUser
            {
                UserId = "user@example.com",
                Email = "user@example.com",
                Password = "user123",
                Name = "Bob User",
                Roles = new[] { "user" }
            }
        };

        /// <summary>
        /// Initializes the user service with logging support.
        /// </summary>
        /// <param name="logger">Logger for authentication events and debugging</param>
        public UserService(ILogger<UserService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a user with email and password credentials.
        /// Validates user credentials and returns user information if successful.
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <returns>User information if authentication succeeds, null if credentials are invalid</returns>
        public Task<UserInfoDto?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Authentication attempted with empty email or password");
                return Task.FromResult<UserInfoDto?>(null);
            }

            _logger.LogInformation("Attempting authentication for user: {Email}", email);

            // Find user by email (case-insensitive)
            var user = DemoUsers.FirstOrDefault(u => 
                string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                _logger.LogWarning("Authentication failed: User not found for email: {Email}", email);
                return Task.FromResult<UserInfoDto?>(null);
            }

            // Validate password
            // NOTE: In production, use proper password hashing (e.g., bcrypt, Argon2)
            if (!string.Equals(user.Password, password, StringComparison.Ordinal))
            {
                _logger.LogWarning("Authentication failed: Invalid password for user: {Email}", email);
                return Task.FromResult<UserInfoDto?>(null);
            }

            _logger.LogInformation("Authentication successful for user: {Email} with roles: {Roles}", 
                email, string.Join(", ", user.Roles));

            // Return user information (excluding password)
            return Task.FromResult<UserInfoDto?>(new UserInfoDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Roles = user.Roles
            });
        }

        /// <summary>
        /// Retrieves user information by user ID.
        /// Used for token validation and user context resolution.
        /// </summary>
        /// <param name="userId">Unique user identifier</param>
        /// <returns>User information if found, null otherwise</returns>
        public Task<UserInfoDto?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Task.FromResult<UserInfoDto?>(null);

            _logger.LogDebug("Retrieving user information for ID: {UserId}", userId);

            var user = DemoUsers.FirstOrDefault(u => 
                string.Equals(u.UserId, userId, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                _logger.LogDebug("User not found for ID: {UserId}", userId);
                return Task.FromResult<UserInfoDto?>(null);
            }

            return Task.FromResult<UserInfoDto?>(new UserInfoDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Roles = user.Roles
            });
        }

        /// <summary>
        /// Checks if a user has a specific role.
        /// Used for authorization and access control decisions.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public async Task<bool> UserHasRoleAsync(string userId, string role)
        {
            var user = await GetUserByIdAsync(userId);
            return user?.Roles.Contains(role, StringComparer.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Gets all available demo users for development/testing purposes.
        /// Returns user information without passwords for security.
        /// </summary>
        /// <returns>List of available demo users</returns>
        public async Task<IEnumerable<UserInfoDto>> GetDemoUsersAsync()
        {
            _logger.LogInformation("Retrieving demo users for development purposes");

            return await Task.FromResult(DemoUsers.Select(u => new UserInfoDto
            {
                UserId = u.UserId,
                Email = u.Email,
                Name = u.Name,
                Roles = u.Roles
            }).ToList());
        }
    }

    /// <summary>
    /// Interface for user authentication and management operations.
    /// Defines the contract for user-related services in the library system.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Authenticates user credentials and returns user information.
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>User information if valid, null if invalid</returns>
        Task<UserInfoDto?> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Retrieves user information by ID.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User information if found</returns>
        Task<UserInfoDto?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Checks if user has a specific role.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="role">Role to check</param>
        /// <returns>True if user has role</returns>
        Task<bool> UserHasRoleAsync(string userId, string role);

        /// <summary>
        /// Gets demo users for development.
        /// </summary>
        /// <returns>Available demo users</returns>
        Task<IEnumerable<UserInfoDto>> GetDemoUsersAsync();
    }

    /// <summary>
    /// Internal demo user model for development purposes.
    /// Contains user credentials and profile information.
    /// </summary>
    internal class DemoUser
    {
        public required string UserId { get; init; }
        public required string Email { get; init; }
        public required string Password { get; init; } // In production: hash this properly
        public string? Name { get; init; }
        public required string[] Roles { get; init; }
    }
}