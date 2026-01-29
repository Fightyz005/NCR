using Microsoft.Data.SqlClient;
using NCRManagementSystem.Data;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Repositories.Interfaces;
using System.Data;

namespace NCRManagementSystem.Repositories.Implementations
{
    public class PrPriceRequestRepository : IPrPriceRequestRepository
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<PrPriceRequestRepository> _logger;

        public PrPriceRequestRepository(DbConnection dbConnection, ILogger<PrPriceRequestRepository> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<int> CreateAsync(PrPriceRequest request)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO PrPriceRequests (PrNumber, RequestDate, RequestBy, Department, VendorEmail, Status, Remarks, CreatedDate, CreatedBy)
                VALUES (@PrNumber, @RequestDate, @RequestBy, @Department, @VendorEmail, @Status, @Remarks, @CreatedDate, @CreatedBy);
                SELECT SCOPE_IDENTITY();";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@PrNumber", request.PrNumber),
                _dbConnection.CreateParameter("@RequestDate", request.RequestDate),
                _dbConnection.CreateParameter("@RequestBy", request.RequestBy),
                _dbConnection.CreateParameter("@Department", request.Department),
                _dbConnection.CreateParameter("@VendorEmail", request.VendorEmail),
                _dbConnection.CreateParameter("@Status", request.Status),
                _dbConnection.CreateParameter("@Remarks", request.Remarks),
                _dbConnection.CreateParameter("@CreatedDate", request.CreatedDate),
                _dbConnection.CreateParameter("@CreatedBy", request.CreatedBy));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> CreateItemAsync(PrPriceRequestItem item)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO PrPriceRequestItems (RequestId, Plant, MaterialCode, MaterialName, Quantity, Unit, Remark, Status, CreatedDate)
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

        public async Task<int> CreateFileAsync(PrPriceRequestFile file)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = @"
                INSERT INTO PrPriceRequestFiles (ItemId, FileName, OriginalFileName, FilePath, FileSize, FileType, UploadedDate, UploadedBy)
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

        public async Task<PrPriceRequest?> GetByIdAsync(int requestId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            var sql = "SELECT * FROM PrPriceRequests WHERE RequestId = @RequestId";
            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@RequestId", requestId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PrPriceRequest
                {
                    RequestId = reader.GetInt32("RequestId"),
                    PrNumber = reader.GetString("PrNumber"),
                    RequestDate = reader.GetDateTime("RequestDate"),
                    RequestBy = reader.GetInt32("RequestBy"),
                    Department = reader.IsDBNull("Department") ? null : reader.GetString("Department"),
                    VendorEmail = reader.IsDBNull("VendorEmail") ? null : reader.GetString("VendorEmail"),
                    Status = reader.GetString("Status"),
                    Remarks = reader.IsDBNull("Remarks") ? null : reader.GetString("Remarks"),
                    CreatedDate = reader.GetDateTime("CreatedDate"),
                    CreatedBy = reader.GetInt32("CreatedBy")
                };
            }
            return null;
        }

        public async Task<PrPriceRequestDto?> GetDetailsAsync(int requestId)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
            // Header
            var sqlHeader = @"
                SELECT r.*, u.FullName as RequestByName 
                FROM PrPriceRequests r 
                LEFT JOIN Users u ON r.RequestBy = u.UserId
                WHERE r.RequestId = @RequestId";
            
            PrPriceRequestDto? result = null;
            
            using (var cmdHeader = _dbConnection.CreateCommand(sqlHeader, connection, _dbConnection.CreateParameter("@RequestId", requestId)))
            using (var reader = await cmdHeader.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    result = new PrPriceRequestDto
                    {
                        RequestId = reader.GetInt32("RequestId"),
                        PrNumber = reader.GetString("PrNumber"),
                        RequestDate = reader.GetDateTime("RequestDate"),
                        RequestByName = reader.IsDBNull("RequestByName") ? "" : reader.GetString("RequestByName"),
                        VendorEmail = reader.IsDBNull("VendorEmail") ? null : reader.GetString("VendorEmail"),
                        Status = reader.GetString("Status")
                    };
                }
            }

            if (result == null) return null;

            // Items
            var sqlItems = "SELECT * FROM PrPriceRequestItems WHERE RequestId = @RequestId";
            using (var cmdItems = _dbConnection.CreateCommand(sqlItems, connection, _dbConnection.CreateParameter("@RequestId", requestId)))
            using (var reader = await cmdItems.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Items.Add(new PrPriceRequestItemDto
                    {
                        ItemId = reader.GetInt32("ItemId"),
                        MaterialCode = reader.GetString("MaterialCode"),
                        MaterialName = reader.GetString("MaterialName"),
                        Quantity = reader.IsDBNull("Quantity") ? null : reader.GetDecimal("Quantity"),
                        Unit = reader.IsDBNull("Unit") ? null : reader.GetString("Unit"),
                        Remark = reader.IsDBNull("Remark") ? null : reader.GetString("Remark"),
                        UnitPrice = reader.IsDBNull("UnitPrice") ? null : reader.GetDecimal("UnitPrice"),
                        Currency = reader.IsDBNull("Currency") ? null : reader.GetString("Currency"),
                        SupplierName = reader.IsDBNull("SupplierName") ? null : reader.GetString("SupplierName"),
                        Status = reader.GetString("Status")
                    });
                }
            }

            result.TotalItems = result.Items.Count;
            result.PricedItems = result.Items.Count(i => i.Status == "Priced");

            return result;
        }

        public async Task<string> GenerateRequestNumberAsync()
        {
            // Just a placeholder, actually might not be needed if PR Number is the key
            return ""; 
        }

        public async Task<PagedResult<PrPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? status = null)
        {
            var result = new PagedResult<PrPriceRequestDto>
            {
                Data = new List<PrPriceRequestDto>(),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            using var connection = await _dbConnection.CreateConnectionAsync();
            var offset = (pageNumber - 1) * pageSize;
            
            var sql = @"
                SELECT r.*, u.FullName as RequestByName,
                COUNT(*) OVER() as TotalRecords
                FROM PrPriceRequests r
                LEFT JOIN Users u ON r.RequestBy = u.UserId
                WHERE (@SearchTerm IS NULL OR r.PrNumber LIKE '%' + @SearchTerm + '%')
                AND (@Status IS NULL OR r.Status = @Status)
                ORDER BY r.RequestDate DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            using var command = _dbConnection.CreateCommand(sql, connection,
                _dbConnection.CreateParameter("@SearchTerm", searchTerm),
                _dbConnection.CreateParameter("@Status", status),
                _dbConnection.CreateParameter("@Offset", offset),
                _dbConnection.CreateParameter("@PageSize", pageSize));

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.TotalRecords = reader.GetInt32("TotalRecords");
                result.Data.Add(new PrPriceRequestDto
                {
                    RequestId = reader.GetInt32("RequestId"),
                    PrNumber = reader.GetString("PrNumber"),
                    RequestDate = reader.GetDateTime("RequestDate"),
                    RequestByName = reader.IsDBNull("RequestByName") ? "" : reader.GetString("RequestByName"),
                    VendorEmail = reader.IsDBNull("VendorEmail") ? null : reader.GetString("VendorEmail"),
                    Status = reader.GetString("Status")
                });
            }

            return result;
        }

        public async Task<bool> UpdateStatusAsync(int requestId, string status, int updatedBy)
        {
             using var connection = await _dbConnection.CreateConnectionAsync();
             var sql = @"UPDATE PrPriceRequests SET Status = @Status, UpdatedDate = GETDATE(), UpdatedBy = @UpdatedBy WHERE RequestId = @RequestId";
             using var command = _dbConnection.CreateCommand(sql, connection,
                 _dbConnection.CreateParameter("@Status", status),
                 _dbConnection.CreateParameter("@UpdatedBy", updatedBy),
                 _dbConnection.CreateParameter("@RequestId", requestId));
             return (await command.ExecuteNonQueryAsync()) > 0;
        }

        public async Task<bool> UpdateItemPriceAsync(int itemId, decimal unitPrice, string currency, string? supplierName, int? leadTimeDays, string? priceRemark, int updatedBy)
        {
            using var connection = await _dbConnection.CreateConnectionAsync();
             var sql = @"UPDATE PrPriceRequestItems SET 
                UnitPrice = @UnitPrice, Currency = @Currency, SupplierName = @SupplierName, 
                LeadTimeDays = @LeadTimeDays, PriceRemark = @PriceRemark, 
                PriceUpdatedDate = GETDATE(), PriceUpdatedBy = @UpdatedBy, Status = 'Priced'
                WHERE ItemId = @ItemId";
             
             using var command = _dbConnection.CreateCommand(sql, connection,
                 _dbConnection.CreateParameter("@UnitPrice", unitPrice),
                 _dbConnection.CreateParameter("@Currency", currency),
                 _dbConnection.CreateParameter("@SupplierName", supplierName),
                 _dbConnection.CreateParameter("@LeadTimeDays", leadTimeDays),
                 _dbConnection.CreateParameter("@PriceRemark", priceRemark),
                 _dbConnection.CreateParameter("@UpdatedBy", updatedBy),
                 _dbConnection.CreateParameter("@ItemId", itemId));
             
             return (await command.ExecuteNonQueryAsync()) > 0;
        }
    }
}
