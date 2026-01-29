using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<SupplierPerformanceDto>> GetSupplierPerformanceAsync(DateTime? fromDate = null, DateTime? toDate = null, int? supplierId = null);
        Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<object> GetProblemAnalysisAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportNCRsToExcelAsync(string? status = null, string? grade = null, int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
