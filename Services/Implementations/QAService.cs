using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class QAService : IQAService
    {
        private readonly INCRRepository _ncrRepository;
        private readonly ILogger<QAService> _logger;

        public QAService(INCRRepository ncrRepository, ILogger<QAService> logger)
        {
            _ncrRepository = ncrRepository;
            _logger = logger;
        }

        public async Task<List<PendingTaskDto>> GetPendingReviewsAsync()
        {
            try
            {
                return await _ncrRepository.GetPendingByRoleAsync("QA");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending QA reviews");
                return new List<PendingTaskDto>();
            }
        }

        public async Task<bool> SendNCRToSupplierAsync(int ncrId, string recipientEmail, DateTime dueDate, string? additionalMessage, int qaUserId)
        {
            try
            {
                return await _ncrRepository.UpdateStatusAsync(ncrId, "Sent", qaUserId, additionalMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending NCR {NCRId} to supplier", ncrId);
                return false;
            }
        }

        public async Task<bool> RejectNCRAsync(int ncrId, string reason, int qaUserId)
        {
            try
            {
                return await _ncrRepository.UpdateStatusAsync(ncrId, "Rejected", qaUserId, reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting NCR {NCRId}", ncrId);
                return false;
            }
        }
    }
}
