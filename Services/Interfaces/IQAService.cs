using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IQAService
    {
        Task<List<PendingTaskDto>> GetPendingReviewsAsync();
        Task<bool> SendNCRToSupplierAsync(int ncrId, string recipientEmail, DateTime dueDate, string? additionalMessage, int qaUserId);
        Task<bool> RejectNCRAsync(int ncrId, string reason, int qaUserId);
    }
}
