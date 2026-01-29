using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface INCRService
    {
        Task<NCRDto?> GetNCRDetailsAsync(int ncrId);
        Task<PagedResult<NCRDto>> GetPagedNCRsAsync(int pageNumber, int pageSize, string? searchTerm = null,
            string? status = null, string? grade = null, int? supplierId = null,
            DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, string? userRole = null);
        Task<List<NCRDto>> GetRecentNCRsAsync(int count = 10);
        Task<List<PendingTaskDto>> GetPendingNCRsAsync(string userRole, int? userId = null);
        Task<int> CreateNCRAsync(NCR ncr);
        Task<bool> UpdateNCRAsync(NCR ncr);
        Task<bool> UpdateNCRStatusAsync(int ncrId, string newStatus, int updatedBy, string? comments = null);
        Task<bool> DeleteNCRAsync(int ncrId);
        Task<string> GenerateNCRNumberAsync();
        Task<bool> AddCommentAsync(int ncrId, string commentText, string commentType, int userId);
    }
}
