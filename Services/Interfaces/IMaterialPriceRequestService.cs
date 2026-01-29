// Services/Interfaces/IMaterialPriceRequestService.cs
using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;
using NCRManagementSystem.Models.ViewModels;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IMaterialPriceRequestService
    {
        Task<MaterialPriceRequestDto?> GetDetailsAsync(int requestId);
        Task<PagedResult<MaterialPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? status = null,
            DateTime? fromDate = null, DateTime? toDate = null,
            int? userId = null, string? userRole = null);
        Task<MaterialPriceRequestStatsDto> GetStatsAsync(int? userId = null, string? userRole = null);
        Task<int> CreateRequestAsync(MaterialPriceRequestCreateViewModel model, int userId, string? department);
        Task<bool> UpdatePricesAsync(int requestId, List<PriceUpdateItemModel> items, int userId);
        Task<bool> CompleteRequestAsync(int requestId, int userId);
        Task<bool> CancelRequestAsync(int requestId, int userId);
        Task<string> GenerateRequestNumberAsync();


        // File operations
        Task<MaterialPriceRequestFileDto?> SaveFileAsync(IFormFile file, int itemId, int uploadedBy);
        Task<bool> DeleteFileAsync(int fileId);
        Task<(byte[]? content, string? fileName, string? contentType)> GetFileContentAsync(int fileId);

        // Notification
        Task SendNotificationToPurchasingAsync(int requestId);
        Task SendNotificationToProductionAsync(int requestId);
        Task ProcessPendingRemindersAsync();
    }
}