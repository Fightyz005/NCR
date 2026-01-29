using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> ValidateUserAsync(string username, string password, string expectedRole);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<List<UserDto>> GetUsersByRoleAsync(string role);
    }
}
