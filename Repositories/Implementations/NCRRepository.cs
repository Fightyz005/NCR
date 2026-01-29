using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class NCRRepository : INCRRepository
    {
        private readonly DbConnection _dbConnection;

        public NCRRepository(DbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<NCR?> GetByIdAsync(int ncrId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetNCRDetails @NCRId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapNCR(reader);
            }
            return null;
        }

        public async Task<NCR?> GetByNumberAsync(string ncrNumber)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM NCRs WHERE NCRNumber = @NCRNumber";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRNumber", ncrNumber));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapNCR(reader);
            }
            return null;
        }

        public async Task<PagedResult<NCRDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null,
            string? status = null, string? grade = null, int? supplierId = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, string? userRole = null)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetNCRList @PageNumber, @PageSize, @SearchTerm, @Status, @Grade, @SupplierId, @FromDate, @ToDate, @UserId, @UserRole";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@PageNumber", pageNumber),
                _dbConnection.CreateParameter("@PageSize", pageSize),
                _dbConnection.CreateParameter("@SearchTerm", searchTerm),
                _dbConnection.CreateParameter("@Status", status),
                _dbConnection.CreateParameter("@Grade", grade),
                _dbConnection.CreateParameter("@SupplierId", supplierId),
                _dbConnection.CreateParameter("@FromDate", fromDate),
                _dbConnection.CreateParameter("@ToDate", toDate),
                _dbConnection.CreateParameter("@UserId", userId),
                _dbConnection.CreateParameter("@UserRole", userRole));

            var result = new PagedResult<NCRDto>
            {
                Data = new List<NCRDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var ncrDto = MapNCRDto(reader);
                    result.Data.Add(ncrDto);
                    result.TotalRecords = reader.GetInt32("TotalRecords");
                }
            }
            catch(Exception ex)
            {
                throw;
            }

            return result;
        }

        public async Task<List<NCRDto>> GetRecentAsync(int count = 10)
        {
            var ncrs = new List<NCRDto>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT TOP (@Count) n.*, s.SupplierName, u.FullName AS CreatedBy
                FROM NCRs n
                INNER JOIN Suppliers s ON n.SupplierId = s.SupplierId
                INNER JOIN Users u ON n.CreatedBy = u.UserId
                ORDER BY n.CreatedDate DESC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@Count", count));
            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    ncrs.Add(MapNCRDto(reader));
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return ncrs;
        }

        public async Task<List<PendingTaskDto>> GetPendingByRoleAsync(string userRole, int? userId = null)
        {
            var tasks = new List<PendingTaskDto>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetPendingNCRsForRole @UserRole, @UserId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserRole", userRole),
                _dbConnection.CreateParameter("@UserId", userId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                tasks.Add(MapPendingTaskDto(reader));
            }
            return tasks;
        }

        public async Task<int> CreateAsync(NCR ncr)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO NCRs (NCRNumber, ProductName, ItemCode, SupplierId, LotNumber, Grade, Priority, 
                                 ProblemDescription, Status, CreatedBy, CreatedDate, DueDate)
                VALUES (@NCRNumber, @ProductName, @ItemCode, @SupplierId, @LotNumber, @Grade, @Priority,
                        @ProblemDescription, @Status, @CreatedBy, @CreatedDate, @DueDate);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRNumber", ncr.NCRNumber),
                _dbConnection.CreateParameter("@ProductName", ncr.ProductName),
                _dbConnection.CreateParameter("@ItemCode", ncr.ItemCode),
                _dbConnection.CreateParameter("@SupplierId", ncr.SupplierId),
                _dbConnection.CreateParameter("@LotNumber", ncr.LotNumber),
                _dbConnection.CreateParameter("@Grade", ncr.Grade),
                _dbConnection.CreateParameter("@Priority", ncr.Priority),
                _dbConnection.CreateParameter("@ProblemDescription", ncr.ProblemDescription),
                _dbConnection.CreateParameter("@Status", ncr.Status),
                _dbConnection.CreateParameter("@CreatedBy", ncr.CreatedBy),
                _dbConnection.CreateParameter("@CreatedDate", ncr.CreatedDate),
                _dbConnection.CreateParameter("@DueDate", ncr.DueDate));

            var result = await command.ExecuteScalarAsync();
            var ncrId = Convert.ToInt32(result);

            // Add to history
            await AddHistoryAsync(ncrId, "Created", "NCR created", null, "New", ncr.CreatedBy);

            return ncrId;
        }

        public async Task<bool> UpdateAsync(NCR ncr)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE NCRs 
                SET ProductName = @ProductName, ItemCode = @ItemCode, SupplierId = @SupplierId,
                    LotNumber = @LotNumber, Grade = @Grade, Priority = @Priority,
                    ProblemDescription = @ProblemDescription, Status = @Status,
                    QAReviewedDate = @QAReviewedDate, QAReviewedBy = @QAReviewedBy, QAComments = @QAComments,
                    SupplierResponseDate = @SupplierResponseDate, RootCause = @RootCause,
                    CorrectiveAction = @CorrectiveAction, PreventiveAction = @PreventiveAction,
                    CompletionDate = @CompletionDate, ResponsiblePerson = @ResponsiblePerson,
                    ManagerApprovedDate = @ManagerApprovedDate, ManagerApprovedBy = @ManagerApprovedBy,
                    ManagerComments = @ManagerComments, ClosedDate = @ClosedDate, ClosedBy = @ClosedBy,
                    UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                WHERE NCRId = @NCRId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncr.NCRId),
                _dbConnection.CreateParameter("@ProductName", ncr.ProductName),
                _dbConnection.CreateParameter("@ItemCode", ncr.ItemCode),
                _dbConnection.CreateParameter("@SupplierId", ncr.SupplierId),
                _dbConnection.CreateParameter("@LotNumber", ncr.LotNumber),
                _dbConnection.CreateParameter("@Grade", ncr.Grade),
                _dbConnection.CreateParameter("@Priority", ncr.Priority),
                _dbConnection.CreateParameter("@ProblemDescription", ncr.ProblemDescription),
                _dbConnection.CreateParameter("@Status", ncr.Status),
                _dbConnection.CreateParameter("@QAReviewedDate", ncr.QAReviewedDate),
                _dbConnection.CreateParameter("@QAReviewedBy", ncr.QAReviewedBy),
                _dbConnection.CreateParameter("@QAComments", ncr.QAComments),
                _dbConnection.CreateParameter("@SupplierResponseDate", ncr.SupplierResponseDate),
                _dbConnection.CreateParameter("@RootCause", ncr.RootCause),
                _dbConnection.CreateParameter("@CorrectiveAction", ncr.CorrectiveAction),
                _dbConnection.CreateParameter("@PreventiveAction", ncr.PreventiveAction),
                _dbConnection.CreateParameter("@CompletionDate", ncr.CompletionDate),
                _dbConnection.CreateParameter("@ResponsiblePerson", ncr.ResponsiblePerson),
                _dbConnection.CreateParameter("@ManagerApprovedDate", ncr.ManagerApprovedDate),
                _dbConnection.CreateParameter("@ManagerApprovedBy", ncr.ManagerApprovedBy),
                _dbConnection.CreateParameter("@ManagerComments", ncr.ManagerComments),
                _dbConnection.CreateParameter("@ClosedDate", ncr.ClosedDate),
                _dbConnection.CreateParameter("@ClosedBy", ncr.ClosedBy),
                _dbConnection.CreateParameter("@UpdatedDate", DateTime.Now),
                _dbConnection.CreateParameter("@UpdatedBy", ncr.UpdatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateStatusAsync(int ncrId, string newStatus, int updatedBy, string? comments = null)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_UpdateNCRStatus @NCRId, @NewStatus, @UpdatedBy, @Comments";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId),
                _dbConnection.CreateParameter("@NewStatus", newStatus),
                _dbConnection.CreateParameter("@UpdatedBy", updatedBy),
                _dbConnection.CreateParameter("@Comments", comments));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int ncrId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Delete related records first
                var deleteFiles = "DELETE FROM NCRFiles WHERE NCRId = @NCRId";
                using var fileCmd = _dbConnection.CreateCommand(deleteFiles, connection,
                    _dbConnection.CreateParameter("@NCRId", ncrId));
                fileCmd.Transaction = transaction;
                await fileCmd.ExecuteNonQueryAsync();

                var deleteHistory = "DELETE FROM NCRHistory WHERE NCRId = @NCRId";
                using var historyCmd = _dbConnection.CreateCommand(deleteHistory, connection,
                    _dbConnection.CreateParameter("@NCRId", ncrId));
                historyCmd.Transaction = transaction;
                await historyCmd.ExecuteNonQueryAsync();

                var deleteComments = "DELETE FROM NCRComments WHERE NCRId = @NCRId";
                using var commentCmd = _dbConnection.CreateCommand(deleteComments, connection,
                    _dbConnection.CreateParameter("@NCRId", ncrId));
                commentCmd.Transaction = transaction;
                await commentCmd.ExecuteNonQueryAsync();

                // Delete NCR
                var deleteNCR = "DELETE FROM NCRs WHERE NCRId = @NCRId";
                using var ncrCmd = _dbConnection.CreateCommand(deleteNCR, connection,
                    _dbConnection.CreateParameter("@NCRId", ncrId));
                ncrCmd.Transaction = transaction;
                var rowsAffected = await ncrCmd.ExecuteNonQueryAsync();

                transaction.Commit();
                return rowsAffected > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<string> GenerateNCRNumberAsync()
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GenerateNCRNumber";
            using var command = _dbConnection.CreateCommand(sql, connection);

            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "NCR-2025-001";
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int? userId = null, string? userRole = null)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetDashboardStats @UserId, @UserRole";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", userId),
                _dbConnection.CreateParameter("@UserRole", userRole));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DashboardStatsDto
                {
                    TotalNCRs = reader.GetInt32("TotalNCRs"),
                    OpenNCRs = reader.GetInt32("OpenNCRs"),
                    ClosedNCRs = reader.GetInt32("ClosedNCRs"),
                    MonthlyNCRs = reader.GetInt32("MonthlyNCRs"),
                    GradeACount = reader.GetInt32("GradeACount"),
                    GradeBCount = reader.GetInt32("GradeBCount"),
                    GradeCCount = reader.GetInt32("GradeCCount")
                };
            }
            return new DashboardStatsDto();
        }

        private async Task AddHistoryAsync(int ncrId, string action, string description, string? oldStatus, string? newStatus, int actionBy)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_InsertNCRHistory @NCRId, @Action, @Description, @OldStatus, @NewStatus, @ActionBy";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@NCRId", ncrId),
                _dbConnection.CreateParameter("@Action", action),
                _dbConnection.CreateParameter("@Description", description),
                _dbConnection.CreateParameter("@OldStatus", oldStatus),
                _dbConnection.CreateParameter("@NewStatus", newStatus),
                _dbConnection.CreateParameter("@ActionBy", actionBy));

            await command.ExecuteNonQueryAsync();
        }

        private static NCR MapNCR(SqlDataReader reader)
        {
            return new NCR
            {
                NCRId = reader.GetInt32("NCRId"),
                NCRNumber = reader.GetString("NCRNumber"),
                ProductName = reader.GetString("ProductName"),
                ItemCode = reader.IsDBNull("ItemCode") ? null : reader.GetString("ItemCode"),
                SupplierId = reader.IsDBNull("SupplierId") ? 0 : reader.GetInt32("SupplierId"),
                LotNumber = reader.IsDBNull("LotNumber") ? null : reader.GetString("LotNumber"),
                Grade = reader.GetString("Grade"),
                Priority = reader.GetString("Priority"),
                ProblemDescription = reader.GetString("ProblemDescription"),
                Status = reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                DueDate = reader.IsDBNull("DueDate") ? null : reader.GetDateTime("DueDate"),
                QAReviewedDate = reader.IsDBNull("QAReviewedDate") ? null : reader.GetDateTime("QAReviewedDate"),
                QAReviewedBy = reader.IsDBNull("QAReviewedBy") ? null : reader.GetInt32("QAReviewedBy"),
                QAComments = reader.IsDBNull("QAComments") ? null : reader.GetString("QAComments"),
                SupplierResponseDate = reader.IsDBNull("SupplierResponseDate") ? null : reader.GetDateTime("SupplierResponseDate"),
                RootCause = reader.IsDBNull("RootCause") ? null : reader.GetString("RootCause"),
                CorrectiveAction = reader.IsDBNull("CorrectiveAction") ? null : reader.GetString("CorrectiveAction"),
                PreventiveAction = reader.IsDBNull("PreventiveAction") ? null : reader.GetString("PreventiveAction"),
                CompletionDate = reader.IsDBNull("CompletionDate") ? null : reader.GetDateTime("CompletionDate"),
                ResponsiblePerson = reader.IsDBNull("ResponsiblePerson") ? null : reader.GetString("ResponsiblePerson"),
                ManagerApprovedDate = reader.IsDBNull("ManagerApprovedDate") ? null : reader.GetDateTime("ManagerApprovedDate"),
                ManagerApprovedBy = reader.IsDBNull("ManagerApprovedBy") ? null : reader.GetInt32("ManagerApprovedBy"),
                ManagerComments = reader.IsDBNull("ManagerComments") ? null : reader.GetString("ManagerComments"),
                ClosedDate = reader.IsDBNull("ClosedDate") ? null : reader.GetDateTime("ClosedDate"),
                ClosedBy = reader.IsDBNull("ClosedBy") ? null : reader.GetInt32("ClosedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy")
            };
        }

        private NCRDto MapNCRDto(SqlDataReader reader)
        {
            return new NCRDto
            {
                NCRId = GetSafeInt32(reader, "NCRId"),
                NCRNumber = GetSafeString(reader, "NCRNumber"),
                ProductName = GetSafeString(reader, "ProductName"),
                ItemCode = GetSafeString(reader, "ItemCode"),
                SupplierId = GetSafeInt32(reader, "SupplierId"),
                LotNumber = GetSafeString(reader, "LotNumber"),
                Grade = GetSafeString(reader, "Grade"),
                Priority = GetSafeString(reader, "Priority"),
                ProblemDescription = GetSafeString(reader, "ProblemDescription"),
                Status = GetSafeString(reader, "Status"),
                CreatedDate = GetSafeDateTime(reader, "CreatedDate"),
                CreatedBy = GetSafeString(reader, "CreatedBy"),
                SupplierName = GetSafeString(reader, "SupplierName"),
                DueDate = reader.IsDBNull("DueDate") ? null : reader.GetDateTime("DueDate")
            };
        }

        // Helper methods - เพิ่มในคลาส NCRRepository
        private bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private string GetSafeString(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return string.Empty;
            if (reader.IsDBNull(columnName)) return string.Empty;

            // รองรับ type ต่างๆ
            var value = reader[columnName];
            return value?.ToString() ?? string.Empty;
        }

        private int GetSafeInt32(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return 0;
            if (reader.IsDBNull(columnName)) return 0;

            var value = reader[columnName];
            if (value is int intValue) return intValue;
            if (int.TryParse(value?.ToString(), out int parsedValue)) return parsedValue;
            return 0;
        }

        private DateTime GetSafeDateTime(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return DateTime.Now;
            if (reader.IsDBNull(columnName)) return DateTime.Now;

            var value = reader[columnName];
            if (value is DateTime dateValue) return dateValue;
            if (DateTime.TryParse(value?.ToString(), out DateTime parsedValue)) return parsedValue;
            return DateTime.Now;
        }

        // เพิ่ม method สำหรับ nullable int
        private int? GetSafeNullableInt32(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return null;
            if (reader.IsDBNull(columnName)) return null;

            var value = reader[columnName];
            if (value is int intValue) return intValue;
            if (int.TryParse(value?.ToString(), out int parsedValue)) return parsedValue;
            return null;
        }

        // เพิ่ม method สำหรับ nullable DateTime
        private DateTime? GetSafeNullableDateTime(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return null;
            if (reader.IsDBNull(columnName)) return null;

            var value = reader[columnName];
            if (value is DateTime dateValue) return dateValue;
            if (DateTime.TryParse(value?.ToString(), out DateTime parsedValue)) return parsedValue;
            return null;
        }

        // เพิ่ม method สำหรับ boolean
        private bool GetSafeBoolean(SqlDataReader reader, string columnName)
        {
            if (!HasColumn(reader, columnName)) return false;
            if (reader.IsDBNull(columnName)) return false;

            var value = reader[columnName];
            if (value is bool boolValue) return boolValue;
            if (value is int intValue) return intValue != 0;
            if (bool.TryParse(value?.ToString(), out bool parsedValue)) return parsedValue;
            return false;
        }

        private static PendingTaskDto MapPendingTaskDto(SqlDataReader reader)
        {
            DateTime? dueDate = reader.IsDBNull("DueDate") ? (DateTime?)null : reader.GetDateTime("DueDate");
            int? daysRemaining = dueDate.HasValue ? (DateTime.Now.Date - dueDate.Value.Date).Days * -1 : (int?)null;

            return new PendingTaskDto
            {
                NCRId = reader.GetInt32("NCRId"),
                NCRNumber = reader.GetString("NCRNumber"),
                ProductName = reader.GetString("ProductName"),
                Grade = reader.GetString("Grade"),
                Priority = reader.GetString("Priority"),
                SupplierName = reader.GetString("SupplierName"),
                DueDate = dueDate,
                DaysRemaining = daysRemaining
            };
        }
    }
}
