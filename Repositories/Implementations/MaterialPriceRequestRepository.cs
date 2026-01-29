// Repositories/Implementations/MaterialPriceRequestRepository.cs
using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class MaterialPriceRequestRepository : IMaterialPriceRequestRepository
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<MaterialPriceRequestRepository> _logger;

        public MaterialPriceRequestRepository(DbConnection dbConnection, ILogger<MaterialPriceRequestRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<string> GenerateRequestNumberAsync()
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GenerateMaterialPriceRequestNumber";
            using var command = _dbConnection.CreateCommand(sql, connection);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? $"MPR-{DateTime.Now:yyyyMM}-0001";
        }

        public async Task<MaterialPriceRequest?> GetByIdAsync(int requestId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM MaterialPriceRequests WHERE RequestId = @RequestId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapRequest(reader);
            }
            return null;
        }

        public async Task<MaterialPriceRequestDto?> GetDetailsAsync(int requestId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetMaterialPriceRequestDetails @RequestId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId));

            using var reader = await command.ExecuteReaderAsync();

            MaterialPriceRequestDto? result = null;

            // Read request header
            if (await reader.ReadAsync())
            {
                result = new MaterialPriceRequestDto
                {
                    RequestId = reader.GetInt32("RequestId"),
                    RequestNumber = reader.GetString("RequestNumber"),
                    RequestDate = reader.GetDateTime("RequestDate"),
                    RequestBy = reader.GetInt32("RequestBy"),
                    RequestByName = reader.GetString("RequestByName"),
                    Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                    Status = reader.GetString("Status"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                    CompletedDate = reader.IsDBNull("CompletedDate") ? null : reader.GetDateTime("CompletedDate"),
                    CompletedByName = reader.IsDBNull("CompletedByName") ? null : reader.GetString("CompletedByName")
                };
            }

            if (result == null) return null;

            // Read items
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Items.Add(new MaterialPriceRequestItemDto
                    {
                        ItemId = reader.GetInt32("ItemId"),
                        RequestId = reader.GetInt32("RequestId"),
                        Plant = reader.GetString("Plant"),
                        MaterialCode = reader.GetString("MaterialCode"),
                        MaterialName = reader.GetString("MaterialName"),
                        Quantity = reader.IsDBNull("Quantity") ? null : reader.GetDecimal("Quantity"),
                        Unit = reader.IsDBNull("Unit") ? null : reader.GetString("Unit"),
                        Remark = reader.IsDBNull("Remark") ? null : reader.GetString("Remark"),
                        UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDecimal("UnitPrice"),
                        Currency = reader.IsDBNull("Currency") ? null : reader.GetString("Currency"),
                        SupplierName = reader.IsDBNull("SupplierName") ? null : reader.GetString("SupplierName"),
                        LeadTimeDays = reader.IsDBNull("LeadTimeDays") ? null : reader.GetInt32("LeadTimeDays"),
                        PriceRemark = reader.IsDBNull("PriceRemark") ? null : reader.GetString("PriceRemark"),
                        PriceUpdatedDate = reader.IsDBNull("PriceUpdatedDate") ? null : reader.GetDateTime("PriceUpdatedDate"),
                        PriceUpdatedByName = reader.IsDBNull("PriceUpdatedByName") ? null : reader.GetString("PriceUpdatedByName"),
                        Status = reader.GetString("Status"),
                        CreatedDate = reader.GetDateTime("CreatedDate"),
                        IsOverdue = reader.GetInt32("IsOverdue") == 1
                    });
                }
            }

            // Read files
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    var itemId = reader.GetInt32("ItemId");
                    var item = result.Items.FirstOrDefault(i => i.ItemId == itemId);
                    if (item != null)
                    {
                        item.Files.Add(new MaterialPriceRequestFileDto
                        {
                            FileId = reader.GetInt32("FileId"),
                            ItemId = itemId,
                            FileName = reader.GetString("FileName"),
                            OriginalFileName = reader.GetString("OriginalFileName"),
                            FilePath = reader.GetString("FilePath"),
                            FileSize = reader.GetInt64("FileSize"),
                            FileType = reader.GetString("FileType"),
                            UploadedDate = reader.GetDateTime("UploadedDate"),
                            UploadedByName = reader.GetString("UploadedByName")
                        });
                    }
                }
            }

            // Read history
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.History.Add(new MaterialPriceRequestHistoryDto
                    {
                        HistoryId = reader.GetInt32("HistoryId"),
                        RequestId = reader.GetInt32("RequestId"),
                        Action = reader.GetString("Action"),
                        Description = reader.GetString("Description"),
                        OldStatus = reader.IsDBNull("OldStatus") ? null : reader.GetString("OldStatus"),
                        NewStatus = reader.IsDBNull("NewStatus") ? null : reader.GetString("NewStatus"),
                        ActionDate = reader.GetDateTime("ActionDate"),
                        ActionByName = reader.GetString("ActionByName")
                    });
                }
            }

            // Calculate totals
            result.TotalItems = result.Items.Count;
            result.PricedItems = result.Items.Count(i => i.Status == "Priced");
            result.HasOverdueItems = result.Items.Any(i => i.IsOverdue);

            return result;
        }

        public async Task<PagedResult<MaterialPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? status = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            int? userId = null, string? userRole = null)
        {
            var result = new PagedResult<MaterialPriceRequestDto>
            {
                Data = new List<MaterialPriceRequestDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetMaterialPriceRequestList @PageNumber, @PageSize, @SearchTerm, @Status, @FromDate, @ToDate, @UserId, @UserRole";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@PageNumber", pageNumber),
                _dbConnection.CreateParameter("@PageSize", pageSize),
                _dbConnection.CreateParameter("@SearchTerm", searchTerm),
                _dbConnection.CreateParameter("@Status", status),
                _dbConnection.CreateParameter("@FromDate", fromDate),
                _dbConnection.CreateParameter("@ToDate", toDate),
                _dbConnection.CreateParameter("@UserId", userId),
                _dbConnection.CreateParameter("@UserRole", userRole));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var dto = new MaterialPriceRequestDto
                {
                    RequestId = reader.GetInt32("RequestId"),
                    RequestNumber = reader.GetString("RequestNumber"),
                    RequestDate = reader.GetDateTime("RequestDate"),
                    Status = reader.GetString("Status"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                    CompletedDate = reader.IsDBNull("CompletedDate") ? null : reader.GetDateTime("CompletedDate"),
                    RequestByName = reader.GetString("RequestByName"),
                    Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                    TotalItems = reader.GetInt32("TotalItems"),
                    PricedItems = reader.GetInt32("PricedItems")
                };
                result.Data.Add(dto);
                result.TotalRecords = reader.GetInt32("TotalRecords");
            }

            return result;
        }

        public async Task<MaterialPriceRequestStatsDto> GetStatsAsync(int? userId = null, string? userRole = null)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetMaterialPriceRequestStats @UserId, @UserRole";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@UserId", userId),
                _dbConnection.CreateParameter("@UserRole", userRole));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MaterialPriceRequestStatsDto
                {
                    TotalRequests = reader.GetInt32("TotalRequests"),
                    PendingRequests = reader.GetInt32("PendingRequests"),
                    InProgressRequests = reader.GetInt32("InProgressRequests"),
                    CompletedRequests = reader.GetInt32("CompletedRequests"),
                    MonthlyRequests = reader.GetInt32("MonthlyRequests"),
                    OverdueItems = reader.GetInt32("OverdueItems")
                };
            }
            return new MaterialPriceRequestStatsDto();
        }

        public async Task<int> CreateAsync(MaterialPriceRequest request)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO MaterialPriceRequests (RequestNumber, RequestDate, RequestBy, Department, Status, Remarks, CreatedDate, CreatedBy)
                VALUES (@RequestNumber, @RequestDate, @RequestBy, @Department, @Status, @Remarks, @CreatedDate, @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestNumber", request.RequestNumber),
                _dbConnection.CreateParameter("@RequestDate", request.RequestDate),
                _dbConnection.CreateParameter("@RequestBy", request.RequestBy),
                _dbConnection.CreateParameter("@Department", request.Department),
                _dbConnection.CreateParameter("@Status", request.Status),
                _dbConnection.CreateParameter("@Remarks", request.Remarks),
                _dbConnection.CreateParameter("@CreatedDate", request.CreatedDate),
                _dbConnection.CreateParameter("@CreatedBy", request.CreatedBy));

            var result = await command.ExecuteScalarAsync();
            var requestId = Convert.ToInt32(result);

            // Add history
            await AddHistoryAsync(requestId, "Created", "สร้างใบร้องขอราคาวัตถุดิบ", null, "Pending", request.CreatedBy);

            return requestId;
        }

        public async Task<bool> UpdateAsync(MaterialPriceRequest request)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE MaterialPriceRequests 
                SET Department = @Department, Remarks = @Remarks, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy
                WHERE RequestId = @RequestId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", request.RequestId),
                _dbConnection.CreateParameter("@Department", request.Department),
                _dbConnection.CreateParameter("@Remarks", request.Remarks),
                _dbConnection.CreateParameter("@UpdatedDate", DateTime.Now),
                _dbConnection.CreateParameter("@UpdatedBy", request.UpdatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateStatusAsync(int requestId, string newStatus, int updatedBy)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();

            // Get old status
            var getStatusSql = "SELECT Status FROM MaterialPriceRequests WHERE RequestId = @RequestId";
            using var getStatusCmd = _dbConnection.CreateCommand(getStatusSql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId));
            var oldStatus = (await getStatusCmd.ExecuteScalarAsync())?.ToString();

            var sql = @"
                UPDATE MaterialPriceRequests 
                SET Status = @Status, 
                    CompletedDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE CompletedDate END,
                    CompletedBy = CASE WHEN @Status = 'Completed' THEN @UpdatedBy ELSE CompletedBy END,
                    UpdatedDate = GETDATE(), 
                    UpdatedBy = @UpdatedBy
                WHERE RequestId = @RequestId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId),
                _dbConnection.CreateParameter("@Status", newStatus),
                _dbConnection.CreateParameter("@UpdatedBy", updatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected > 0)
            {
                var description = newStatus switch
                {
                    "InProgress" => "เริ่มดำเนินการอัพเดตราคา",
                    "Completed" => "ปิดงาน - อัพเดตราคาเสร็จสิ้น",
                    "Cancelled" => "ยกเลิกใบร้องขอ",
                    _ => $"เปลี่ยนสถานะเป็น {newStatus}"
                };
                await AddHistoryAsync(requestId, "StatusChanged", description, oldStatus, newStatus, updatedBy);
            }

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int requestId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM MaterialPriceRequests WHERE RequestId = @RequestId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Items
        public async Task<MaterialPriceRequestItem?> GetItemByIdAsync(int itemId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM MaterialPriceRequestItems WHERE ItemId = @ItemId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ItemId", itemId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapItem(reader);
            }
            return null;
        }

        public async Task<int> CreateItemAsync(MaterialPriceRequestItem item)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO MaterialPriceRequestItems (RequestId, Plant, MaterialCode, MaterialName, Quantity, Unit, Remark, Status, CreatedDate)
                VALUES (@RequestId, @Plant, @MaterialCode, @MaterialName, @Quantity, @Unit, @Remark, @Status, @CreatedDate);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", item.RequestId),
                _dbConnection.CreateParameter("@Plant", item.Plant),
                _dbConnection.CreateParameter("@MaterialCode", item.MaterialCode),
                _dbConnection.CreateParameter("@MaterialName", item.MaterialName),
                _dbConnection.CreateParameter("@Quantity", item.Quantity),
                _dbConnection.CreateParameter("@Unit", item.Unit),
                _dbConnection.CreateParameter("@Remark", item.Remark),
                _dbConnection.CreateParameter("@Status", item.Status),
                _dbConnection.CreateParameter("@CreatedDate", item.CreatedDate));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateItemPriceAsync(int itemId, decimal unitPrice, string currency,
            string? supplierName, int? leadTimeDays, string? priceRemark, int updatedBy)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                UPDATE MaterialPriceRequestItems 
                SET UnitPrice = @UnitPrice, Currency = @Currency, SupplierName = @SupplierName,
                    LeadTimeDays = @LeadTimeDays, PriceRemark = @PriceRemark,
                    PriceUpdatedDate = GETDATE(), PriceUpdatedBy = @PriceUpdatedBy, Status = 'Priced'
                WHERE ItemId = @ItemId";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ItemId", itemId),
                _dbConnection.CreateParameter("@UnitPrice", unitPrice),
                _dbConnection.CreateParameter("@Currency", currency),
                _dbConnection.CreateParameter("@SupplierName", supplierName),
                _dbConnection.CreateParameter("@LeadTimeDays", leadTimeDays),
                _dbConnection.CreateParameter("@PriceRemark", priceRemark),
                _dbConnection.CreateParameter("@PriceUpdatedBy", updatedBy));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM MaterialPriceRequestItems WHERE ItemId = @ItemId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ItemId", itemId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Files
        public async Task<int> CreateFileAsync(MaterialPriceRequestFile file)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO MaterialPriceRequestFiles (ItemId, FileName, OriginalFileName, FilePath, FileSize, FileType, UploadedDate, UploadedBy)
                VALUES (@ItemId, @FileName, @OriginalFileName, @FilePath, @FileSize, @FileType, @UploadedDate, @UploadedBy);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ItemId", file.ItemId),
                _dbConnection.CreateParameter("@FileName", file.FileName),
                _dbConnection.CreateParameter("@OriginalFileName", file.OriginalFileName),
                _dbConnection.CreateParameter("@FilePath", file.FilePath),
                _dbConnection.CreateParameter("@FileSize", file.FileSize),
                _dbConnection.CreateParameter("@FileType", file.FileType),
                _dbConnection.CreateParameter("@UploadedDate", file.UploadedDate),
                _dbConnection.CreateParameter("@UploadedBy", file.UploadedBy));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<MaterialPriceRequestFile?> GetFileByIdAsync(int fileId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM MaterialPriceRequestFiles WHERE FileId = @FileId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@FileId", fileId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new MaterialPriceRequestFile
                {
                    FileId = reader.GetInt32("FileId"),
                    ItemId = reader.GetInt32("ItemId"),
                    FileName = reader.GetString("FileName"),
                    OriginalFileName = reader.GetString("OriginalFileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileSize = reader.GetInt64("FileSize"),
                    FileType = reader.GetString("FileType"),
                    UploadedDate = reader.GetDateTime("UploadedDate"),
                    UploadedBy = reader.GetInt32("UploadedBy")
                };
            }
            return null;
        }

        public async Task<List<MaterialPriceRequestFile>> GetFilesByItemIdAsync(int itemId)
        {
            var files = new List<MaterialPriceRequestFile>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM MaterialPriceRequestFiles WHERE ItemId = @ItemId ORDER BY UploadedDate";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@ItemId", itemId));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                files.Add(new MaterialPriceRequestFile
                {
                    FileId = reader.GetInt32("FileId"),
                    ItemId = reader.GetInt32("ItemId"),
                    FileName = reader.GetString("FileName"),
                    OriginalFileName = reader.GetString("OriginalFileName"),
                    FilePath = reader.GetString("FilePath"),
                    FileSize = reader.GetInt64("FileSize"),
                    FileType = reader.GetString("FileType"),
                    UploadedDate = reader.GetDateTime("UploadedDate"),
                    UploadedBy = reader.GetInt32("UploadedBy")
                });
            }
            return files;
        }

        public async Task<bool> DeleteFileAsync(int fileId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "DELETE FROM MaterialPriceRequestFiles WHERE FileId = @FileId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@FileId", fileId));

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // History
        public async Task AddHistoryAsync(int requestId, string action, string description,
            string? oldStatus, string? newStatus, int actionBy)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_InsertMaterialPriceRequestHistory @RequestId, @Action, @Description, @OldStatus, @NewStatus, @ActionBy";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId),
                _dbConnection.CreateParameter("@Action", action),
                _dbConnection.CreateParameter("@Description", description),
                _dbConnection.CreateParameter("@OldStatus", oldStatus),
                _dbConnection.CreateParameter("@NewStatus", newStatus),
                _dbConnection.CreateParameter("@ActionBy", actionBy));

            await command.ExecuteNonQueryAsync();
        }

        // Notifications
        public async Task<List<PendingItemReminderDto>> GetPendingItemsForReminderAsync()
        {
            var items = new List<PendingItemReminderDto>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "EXEC sp_GetPendingItemsForReminder";
            using var command = _dbConnection.CreateCommand(sql, connection);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new PendingItemReminderDto
                {
                    RequestId = reader.GetInt32("RequestId"),
                    RequestNumber = reader.GetString("RequestNumber"),
                    RequestBy = reader.GetInt32("RequestBy"),
                    RequestByName = reader.GetString("RequestByName"),
                    RequestByEmail = reader.GetString("RequestByEmail"),
                    ItemId = reader.GetInt32("ItemId"),
                    MaterialCode = reader.GetString("MaterialCode"),
                    MaterialName = reader.GetString("MaterialName"),
                    ItemCreatedDate = reader.GetDateTime("ItemCreatedDate"),
                    HoursPending = reader.GetInt32("HoursPending")
                });
            }
            return items;
        }

        public async Task CreateNotificationAsync(MaterialPriceRequestNotification notification)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO MaterialPriceRequestNotifications (RequestId, NotificationType, RecipientType, IsSent, CreatedDate)
                VALUES (@RequestId, @NotificationType, @RecipientType, @IsSent, @CreatedDate)";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", notification.RequestId),
                _dbConnection.CreateParameter("@NotificationType", notification.NotificationType),
                _dbConnection.CreateParameter("@RecipientType", notification.RecipientType),
                _dbConnection.CreateParameter("@IsSent", notification.IsSent),
                _dbConnection.CreateParameter("@CreatedDate", notification.CreatedDate));

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<MaterialPriceRequestDto>> GetRecentAsync(int count = 10)
        {
            var requests = new List<MaterialPriceRequestDto>();
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                SELECT TOP (@Count) r.*, u.FullName AS RequestByName,
                    (SELECT COUNT(*) FROM MaterialPriceRequestItems WHERE RequestId = r.RequestId) AS TotalItems,
                    (SELECT COUNT(*) FROM MaterialPriceRequestItems WHERE RequestId = r.RequestId AND Status = 'Priced') AS PricedItems
                FROM MaterialPriceRequests r
                INNER JOIN Users u ON r.RequestBy = u.UserId
                ORDER BY r.RequestDate DESC";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@Count", count));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                requests.Add(new MaterialPriceRequestDto
                {
                    RequestId = reader.GetInt32("RequestId"),
                    RequestNumber = reader.GetString("RequestNumber"),
                    RequestDate = reader.GetDateTime("RequestDate"),
                    Status = reader.GetString("Status"),
                    RequestByName = reader.GetString("RequestByName"),
                    TotalItems = reader.GetInt32("TotalItems"),
                    PricedItems = reader.GetInt32("PricedItems")
                });
            }
            return requests;
        }

        private static MaterialPriceRequest MapRequest(SqlDataReader reader)
        {
            return new MaterialPriceRequest
            {
                RequestId = reader.GetInt32("RequestId"),
                RequestNumber = reader.GetString("RequestNumber"),
                RequestDate = reader.GetDateTime("RequestDate"),
                RequestBy = reader.GetInt32("RequestBy"),
                Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                Status = reader.GetString("Status"),
                Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                CompletedDate = reader.IsDBNull("CompletedDate") ? null : reader.GetDateTime("CompletedDate"),
                CompletedBy = reader.IsDBNull("CompletedBy") ? null : reader.GetInt32("CompletedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                CreatedBy = reader.GetInt32("CreatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? null : reader.GetDateTime("UpdatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetInt32("UpdatedBy")
            };
        }

        private static MaterialPriceRequestItem MapItem(SqlDataReader reader)
        {
            return new MaterialPriceRequestItem
            {
                ItemId = reader.GetInt32("ItemId"),
                RequestId = reader.GetInt32("RequestId"),
                Plant = reader.GetString("Plant"),
                MaterialCode = reader.GetString("MaterialCode"),
                MaterialName = reader.GetString("MaterialName"),
                Quantity = reader.IsDBNull("Quantity") ? null : reader.GetDecimal("Quantity"),
                Unit = reader.IsDBNull("Unit") ? null : reader.GetString("Unit"),
                Remark = reader.IsDBNull("Remark") ? null : reader.GetString("Remark"),
                UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDecimal("UnitPrice"),
                Currency = reader.IsDBNull("Currency") ? null : reader.GetString("Currency"),
                SupplierName = reader.IsDBNull("SupplierName") ? null : reader.GetString("SupplierName"),
                LeadTimeDays = reader.IsDBNull("LeadTimeDays") ? null : reader.GetInt32("LeadTimeDays"),
                PriceRemark = reader.IsDBNull("PriceRemark") ? null : reader.GetString("PriceRemark"),
                PriceUpdatedDate = reader.IsDBNull("PriceUpdatedDate") ? null : reader.GetDateTime("PriceUpdatedDate"),
                PriceUpdatedBy = reader.IsDBNull("PriceUpdatedBy") ? null : reader.GetInt32("PriceUpdatedBy"),
                Status = reader.GetString("Status"),
                CreatedDate = reader.GetDateTime("CreatedDate")
            };
        }
    }
}