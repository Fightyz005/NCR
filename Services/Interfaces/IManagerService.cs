using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IManagerService
    {
        Task<List<PendingTaskDto>> GetPendingApprovalsAsync();
        Task<bool> ApproveNCRAsync(int ncrId, string? comments, int managerId);
        Task<bool> RejectNCRAsync(int ncrId, string reason, int managerId);
    }
}
