using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface INCRRepository
    {
        Task<NCR?> GetByIdAsync(int ncrId);
        Task<NCR?> GetByNumberAsync(string ncrNumber);
        Task<PagedResult<NCRDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null,
            string? status = null, string? grade = null, int? supplierId = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, string? userRole = null);
        Task<List<NCRDto>> GetRecentAsync(int count = 10);
        Task<List<PendingTaskDto>> GetPendingByRoleAsync(string userRole, int? userId = null);
        Task<int> CreateAsync(NCR ncr);
        Task<bool> UpdateAsync(NCR ncr);
        Task<bool> UpdateStatusAsync(int ncrId, string newStatus, int updatedBy, string? comments = null);
        Task<bool> DeleteAsync(int ncrId);
        Task<string> GenerateNCRNumberAsync();
        Task<DashboardStatsDto> GetDashboardStatsAsync(int? userId = null, string? userRole = null);
    }
}
