using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class ManagerService : IManagerService
    {
        private readonly INCRRepository _ncrRepository;
        private readonly ILogger<ManagerService> _logger;

        public ManagerService(INCRRepository ncrRepository, ILogger<ManagerService> logger)
        {
            _ncrRepository = ncrRepository;
            _logger = logger;
        }

        public async Task<List<PendingTaskDto>> GetPendingApprovalsAsync()
        {
            try
            {
                return await _ncrRepository.GetPendingByRoleAsync("Manager");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending manager approvals");
                return new List<PendingTaskDto>();
            }
        }

        public async Task<bool> ApproveNCRAsync(int ncrId, string? comments, int managerId)
        {
            try
            {
                return await _ncrRepository.UpdateStatusAsync(ncrId, "Closed", managerId, comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving NCR {NCRId}", ncrId);
                return false;
            }
        }

        public async Task<bool> RejectNCRAsync(int ncrId, string reason, int managerId)
        {
            try
            {
                return await _ncrRepository.UpdateStatusAsync(ncrId, "Rejected", managerId, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting NCR {NCRId}", ncrId);
                return false;
            }
        }
    }
}
