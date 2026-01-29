using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;  
namespace NCRManagementSystem.Repositories.Implementations
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly DbConnection _dbConnection;

        public SupplierRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Supplier?> GetByIdAsync(int supplierId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Suppliers WHERE SupplierId = @SupplierId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@SupplierId", supplierId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapSupplier(reader);
            }
            return null;
        }

        public async Task<List<Supplier>> GetAllActiveAsync()
        {
            var suppliers = new List<Supplier>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Suppliers WHERE IsActive = 1 ORDER BY SupplierName";
            using var command = _dbConnection.CreateCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapSupplier(reader));
            }
            return suppliers;
        }

        public async Task<List<Supplier>> GetAllAsync()
        {
            var suppliers = new List<Supplier>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM Suppliers ORDER BY SupplierName";
            using var command = _dbConnection.CreateCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                suppliers.Add(MapSupplier(reader));
            }
            return suppliers;
        }

        public async Task<int> CreateAsync(Supplier supplier)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO Suppliers (SupplierCode, SupplierName, ContactPerson, Email, Phone, Address, IsActive, CreatedDate, CreatedBy)
                VALUES (@SupplierCode, @SupplierName, @ContactPerson, @Email, @Phone, @Address, @IsActive, @CreatedDate, @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@SupplierCode", supplier.SupplierCode),
                _dbConnection.CreateParameter("@SupplierName", supplier.SupplierName),
                _dbConnection.CreateParameter("@ContactPerson", supplier.ContactPerson),
                _dbConnection.CreateParameter("@Email", supplier.Email),
                _dbConnection.CreateParameter("@Phone", supplier.Phone),
                _dbConnection.CreateParameter("@Address", supplier.Address),
                _dbConnection.CreateParameter("@IsActive", supplier.IsActive),
                _dbConnection.CreateParameter("@CreatedDate", supplier.CreatedDate),
                _dbConnection.CreateParameter("@CreatedBy", supplier.CreatedBy));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Supplier supplier)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE Suppliers 
                SET SupplierName = @SupplierName, ContactPerson = @ContactPerson, Email = @Email,
                    Phone = @Phone, Address = @Address, IsActive = @IsActive,
                    UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                WHERE SupplierId = @SupplierId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@SupplierId", supplier.SupplierId),
                _dbConnection.CreateParameter("@SupplierName", supplier.SupplierName),
                _dbConnection.CreateParameter("@ContactPerson", supplier.ContactPerson),
                _dbConnection.CreateParameter("@Email", supplier.Email),
                _dbConnection.CreateParameter("@Phone", supplier.Phone),
                _dbConnection.CreateParameter("@Address", supplier.Address),
                _dbConnection.CreateParameter("@IsActive", supplier.IsActive),
                _dbConnection.CreateParameter("@UpdatedDate", DateTime.Now),
                _dbConnection.CreateParameter("@UpdatedBy", supplier.UpdatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int supplierId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "UPDATE Suppliers SET IsActive = 0 WHERE SupplierId = @SupplierId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@SupplierId", supplierId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private static Supplier MapSupplier(SqlDataReader reader)
        {
            return new Supplier
            {
                SupplierId = reader.GetInt32("SupplierId"),
                SupplierCode = reader.GetString("SupplierCode"),
                SupplierName = reader.GetString("SupplierName"),
                ContactPerson = reader.IsDBNull("ContactPerson") ? null : reader.GetString("ContactPerson"),
                Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
                Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                IsActive = reader.GetBoolean("IsActive"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetInt32("CreatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy")
            };
        }
    }
}
