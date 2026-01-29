using NCRManagementSystem.Data;
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Services.Interfaces;
using System.Data;

namespace NCRManagementSystem.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly DbConnection _dbConnection;
        private readonly ILogger<ReportService> _logger;

        public ReportService(DbConnection dbConnection, ILogger<ReportService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
        }

        public async Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, int? supplierId = null)
        {
            try
            {
                var results = new List<SupplierPerformanceDto>();
                using var connection = await _dbConnection.CreateConnectionAsync();
                var sql = "EXEC sp_GetReportData @ReportType, @FromDate, @ToDate, @SupplierId";

                using var command = _dbConnection.CreateCommand(sql, connection,
                    _dbConnection.CreateParameter("@ReportType", "SUPPLIER_PERFORMANCE"),
                    _dbConnection.CreateParameter("@FromDate", fromDate),
                    _dbConnection.CreateParameter("@ToDate", toDate),
                    _dbConnection.CreateParameter("@SupplierId", supplierId));

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new SupplierPerformanceDto
                    {
                        SupplierId = reader.GetInt32("SupplierId"),
                        SupplierName = reader.GetString("SupplierName"),
                        TotalNCRs = reader.GetInt32("TotalNCRs"),
                        GradeACount = reader.GetInt32("GradeACount"),
                        GradeBCount = reader.GetInt32("GradeBCount"),
                        GradeCCount = reader.GetInt32("GradeCCount"),
                        AvgResponseDays = reader.IsDBNull("AvgResponseDays") ? null : reader.GetDecimal("AvgResponseDays"),
                        ClosureRate = reader.GetDecimal("ClosureRate")
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting supplier performance report");
                return new List<SupplierPerformanceDto>();
            }
        }

        public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var results = new List<MonthlyTrendDto>();
                using var connection = await _dbConnection.CreateConnectionAsync();
                var sql = "EXEC sp_GetReportData @ReportType, @FromDate, @ToDate";

                using var command = _dbConnection.CreateCommand(sql, connection,
                    _dbConnection.CreateParameter("@ReportType", "MONTHLY_TREND"),
                    _dbConnection.CreateParameter("@FromDate", fromDate),
                    _dbConnection.CreateParameter("@ToDate", toDate));

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new MonthlyTrendDto
                    {
                        Year = reader.GetInt32("Year"),
                        Month = reader.GetInt32("Month"),
                        MonthName = reader.GetString("MonthName"),
                        NCRCount = reader.GetInt32("NCRCount"),
                        ClosedCount = reader.GetInt32("ClosedCount"),
                        GradeACount = reader.GetInt32("GradeACount"),
                        GradeBCount = reader.GetInt32("GradeBCount"),
                        GradeCCount = reader.GetInt32("GradeCCount")
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly trend report");
                return new List<MonthlyTrendDto>();
            }
        }

        public async Task<object> GetProblemAnalysisAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var results = new List<object>();
                using var connection = await _dbConnection.CreateConnectionAsync();
                var sql = "EXEC sp_GetReportData @ReportType, @FromDate, @ToDate";

                using var command = _dbConnection.CreateCommand(sql, connection,
                    _dbConnection.CreateParameter("@ReportType", "PROBLEM_ANALYSIS"),
                    _dbConnection.CreateParameter("@FromDate", fromDate),
                    _dbConnection.CreateParameter("@ToDate", toDate));

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(new
                    {
                        Grade = reader.GetString("Grade"),
                        Count = reader.GetInt32("Count"),
                        Percentage = reader.GetDecimal("Percentage")
                    });
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting problem analysis report");
                return new List<object>();
            }
        }

        public async Task<byte[]> ExportNCRsToExcelAsync(string? status = null, string? grade = null, int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                // This would use a library like ClosedXML to generate Excel files
                // For now, return empty byte array
                await Task.CompletedTask;
                return new byte[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting NCRs to Excel");
                return new byte[0];
            }
        }
    }
}
