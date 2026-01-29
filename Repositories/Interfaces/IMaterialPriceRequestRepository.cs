// Repositories/Interfaces/IMaterialPriceRequestRepository.cs
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface IMaterialPriceRequestRepository
    {
        Task<MaterialPriceRequest?> GetByIdAsync(int requestId);
        Task<MaterialPriceRequestDto?> GetDetailsAsync(int requestId);
        Task<PagedResult<MaterialPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? status = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            int? userId = null, string? userRole = null);
        Task<List<MaterialPriceRequestDto>> GetRecentAsync(int count = 10);
        Task<MaterialPriceRequestStatsDto> GetStatsAsync(int? userId = null, string? userRole = null);
        Task<int> CreateAsync(MaterialPriceRequest request);
        Task<bool> UpdateAsync(MaterialPriceRequest request);
        Task<bool> UpdateStatusAsync(int requestId, string newStatus, int updatedBy);
        Task<bool> DeleteAsync(int requestId);
        Task<string> GenerateRequestNumberAsync();

        // Items
        Task<MaterialPriceRequestItem?> GetItemByIdAsync(int itemId);
        Task<int> CreateItemAsync(MaterialPriceRequestItem item);
        Task<bool> UpdateItemPriceAsync(int itemId, decimal unitPrice, string currency,
            string? supplierName, int? leadTimeDays, string? priceRemark, int updatedBy);
        Task<bool> DeleteItemAsync(int itemId);

        // Files
        Task<int> CreateFileAsync(MaterialPriceRequestFile file);
        Task<MaterialPriceRequestFile?> GetFileByIdAsync(int fileId);
        Task<List<MaterialPriceRequestFile>> GetFilesByItemIdAsync(int itemId);
        Task<bool> DeleteFileAsync(int fileId);

        // History
        Task AddHistoryAsync(int requestId, string action, string description,
            string? oldStatus, string? newStatus, int actionBy);

        // Notifications
        Task<List<PendingItemReminderDto>> GetPendingItemsForReminderAsync();
        Task CreateNotificationAsync(MaterialPriceRequestNotification notification);
    }

    public class PendingItemReminderDto
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public int RequestBy { get; set; }
        public string RequestByName { get; set; } = string.Empty;
        public string RequestByEmail { get; set; } = string.Empty;
        public int ItemId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public DateTime ItemCreatedDate { get; set; }
        public int HoursPending { get; set; }
    }
}