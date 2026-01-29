using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;
using NCRManagementSystem.Controllers;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnection _dbConnection;
        //private readonly ILogger<DashboardController> _logger;

        public UserRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Users WHERE UserId = @UserId AND IsActive = 1";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", userId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            try
            {
                using var connection = await _dbConnection.CreateConnectionAsync();

                // เพิ่ม log เพื่อดู connection string
                Console.WriteLine($"Connection State: {connection.State}");

                // ตรวจสอบ table Users มีข้อมูลหรือไม่
                var countSql = "SELECT COUNT(*) FROM Users";
                using var countCommand = _dbConnection.CreateCommand(countSql, connection);
                var totalUsers = await countCommand.ExecuteScalarAsync();
                Console.WriteLine($"Total users in database: {totalUsers}");

                // ดู username ที่กำลังค้นหา
                Console.WriteLine($"Searching for username: '{username}'");

                var sql = @"SELECT UserId, Username, Email, PasswordHash, FullName, Role, 
                   Department, IsActive, CreatedDate, CreatedBy, 
                   UpdatedDate, UpdatedBy, LastLoginDate 
            FROM Users 
            WHERE Username = @Username AND IsActive = 1";

                using var command = _dbConnection.CreateCommand(sql, connection,
                    _dbConnection.CreateParameter("@Username", username));

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    Console.WriteLine($"User found: {username}");
                    return MapUser(reader);
                }
                else
                {
                    Console.WriteLine($"User NOT found: {username}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetByUsernameAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Users WHERE Email = @Email AND IsActive = 1";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@Email", email));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapUser(reader);
            }
            return null;
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Users ORDER BY FullName";
            using var command = _dbConnection.CreateCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }
            return users;
        }

        public async Task<List<User>> GetByRoleAsync(string role)
        {
            var users = new List<User>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Users WHERE Role = @Role AND IsActive = 1 ORDER BY FullName";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@Role", role));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                users.Add(MapUser(reader));
            }
            return users;
        }

        public async Task<int> CreateAsync(User user)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, Department, IsActive, CreatedDate, CreatedBy)
                VALUES (@Username, @Email, @PasswordHash, @FullName, @Role, @Department, @IsActive, @CreatedDate, @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@Username", user.Username),
                _dbConnection.CreateParameter("@Email", user.Email),
                _dbConnection.CreateParameter("@PasswordHash", user.PasswordHash),
                _dbConnection.CreateParameter("@FullName", user.FullName),
                _dbConnection.CreateParameter("@Role", user.Role),
                _dbConnection.CreateParameter("@Department", user.Department),
                _dbConnection.CreateParameter("@IsActive", user.IsActive),
                _dbConnection.CreateParameter("@CreatedDate", user.CreatedDate),
                _dbConnection.CreateParameter("@CreatedBy", user.CreatedBy));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE Users 
                SET Email = @Email, FullName = @FullName, Role = @Role, Department = @Department,
                    IsActive = @IsActive, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                WHERE UserId = @UserId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", user.UserId),
                _dbConnection.CreateParameter("@Email", user.Email),
                _dbConnection.CreateParameter("@FullName", user.FullName),
                _dbConnection.CreateParameter("@Role", user.Role),
                _dbConnection.CreateParameter("@Department", user.Department),
                _dbConnection.CreateParameter("@IsActive", user.IsActive),
                _dbConnection.CreateParameter("@UpdatedDate", DateTime.Now),
                _dbConnection.CreateParameter("@UpdatedBy", user.UpdatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateLastLoginAsync(int userId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "UPDATE Users SET LastLoginDate = @LastLoginDate WHERE UserId = @UserId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", userId),
                _dbConnection.CreateParameter("@LastLoginDate", DateTime.Now));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "UPDATE Users SET IsActive = 0 WHERE UserId = @UserId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", userId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return false;

            // Here you would typically use BCrypt or similar to verify password
            // For demo purposes, we'll do a simple comparison
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
        public async Task<int> GetTotalUsersAsync()
        {
            try
            {
                using var connection = await _dbConnection.CreateConnectionAsync();
                var sql = "SELECT COUNT(*) FROM Users WHERE IsActive = 1";
                using var command = _dbConnection.CreateCommand(sql, connection);
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                //_logger?.LogError(ex, "Error getting total users count");
                return 0;
            }
        }
        // เพิ่มใน UserRepository.cs หลัง GetTotalUsersAsync()

        public async Task<bool> CreateDemoUsersIfNotExistAsync()
        {
            try
            {
                using var connection = await _dbConnection.CreateConnectionAsync();

                // ตรวจสอบว่ามี demo users หรือไม่
                var checkSql = "SELECT COUNT(*) FROM Users WHERE Username LIKE 'demo.%'";
                using var checkCommand = _dbConnection.CreateCommand(checkSql, connection);
                var demoCount = (int)await checkCommand.ExecuteScalarAsync();

                if (demoCount > 0)
                {
                    return true; // มี demo users อยู่แล้ว
                }

                // สร้าง BCrypt hash สำหรับ "demo123"
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("demo123");

                var insertSql = @"
                        INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, Department, IsActive, CreatedDate, CreatedBy)
                        VALUES 
                        ('demo.user', 'demo.user@company.com', @PasswordHash, 'Demo User', 'User', 'Supplier Management', 1, GETDATE(), 1),
                        ('demo.qa', 'demo.qa@company.com', @PasswordHash, 'Demo QA', 'QA', 'Quality Assurance', 1, GETDATE(), 1),
                        ('demo.manager', 'demo.manager@company.com', @PasswordHash, 'Demo Manager', 'Manager', 'Management', 1, GETDATE(), 1),
                        ('demo.admin', 'demo.admin@company.com', @PasswordHash, 'Demo Admin', 'Admin', 'IT Department', 1, GETDATE(), 1)";

                using var insertCommand = _dbConnection.CreateCommand(insertSql, connection,
                    _dbConnection.CreateParameter("@PasswordHash", passwordHash));

                var result = await insertCommand.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating demo users: {ex.Message}");
            }
        }
        private User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32("UserId"),
                Username = reader.GetString("Username"),
                Email = reader.GetString("Email"),
                PasswordHash = reader.GetString("PasswordHash"),
                FullName = reader.GetString("FullName"),
                Role = reader.GetString("Role"),
                Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? 0 : reader.GetInt32("CreatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy"),
                LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate")
            };
        }
    }
}
