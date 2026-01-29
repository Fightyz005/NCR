using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int userId);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByRoleAsync(string role);
        Task<int> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> UpdateLastLoginAsync(int userId);
        Task<bool> DeleteAsync(int userId);
        Task<bool> ValidateCredentialsAsync(string username, string password);

        // เพิ่ม methods ใหม่
        Task<int> GetTotalUsersAsync();
        Task<bool> CreateDemoUsersIfNotExistAsync();
    }
}
