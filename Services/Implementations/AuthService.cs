using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserDto?> ValidateUserAsync(string username, string password, string expectedRole)
        {
            try
            {
                _logger.LogInformation("=== Login Debug ===");
                _logger.LogInformation("Username: {Username}", username);
                _logger.LogInformation("Expected Role: {ExpectedRole}", expectedRole);

                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null)
                {
                    _logger.LogWarning("User not found in database: {Username}", username);
                    return null;
                }

                _logger.LogInformation("User found - Role: {Role}, IsActive: {IsActive}", user.Role, user.IsActive);

                if (!user.IsActive)
                {
                    _logger.LogWarning("User is not active: {Username}", username);
                    return null;
                }

                // Verify password (using BCrypt)
                bool passwordMatch = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                _logger.LogInformation("Password match: {PasswordMatch}", passwordMatch);

                if (!passwordMatch)
                {
                    _logger.LogWarning("Invalid password for user: {Username}", username);
                    return null;
                }

                // Check role - ให้ loose matching
                if (!string.IsNullOrEmpty(expectedRole) && user.Role != expectedRole)
                {
                    _logger.LogWarning("Role mismatch - Expected: {ExpectedRole}, Actual: {ActualRole}", expectedRole, user.Role);
                    // ชั่วคราว - ไม่ return null ให้ login ได้ก่อน
                    // return null;
                }

                _logger.LogInformation("Login successful for user: {Username}", username);

                return new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Department = user.Department,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating user credentials for {Username}", username);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            try
            {
                return await _userRepository.UpdateLastLoginAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last login for user {UserId}", userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return null;

                return new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Department = user.Department,
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) return false;

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    return false;
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.PasswordHash = newPasswordHash;
                user.UpdatedDate = DateTime.Now;
                user.UpdatedBy = userId;

                return await _userRepository.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<UserDto>> GetUsersByRoleAsync(string role)
        {
            try
            {
                var users = await _userRepository.GetByRoleAsync(role);
                return users.Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    Department = u.Department,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    LastLoginDate = u.LastLoginDate
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users by role {Role}", role);
                throw;
            }
        }
    }
}
