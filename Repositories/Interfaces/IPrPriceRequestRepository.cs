using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Repositories.Interfaces
{
    public interface IPrPriceRequestRepository
    {
        Task<int> CreateAsync(PrPriceRequest request);
        Task<PrPriceRequest?> GetByIdAsync(int requestId);
        Task<PrPriceRequestDto?> GetDetailsAsync(int requestId);
        Task<int> CreateItemAsync(PrPriceRequestItem item);
        Task<int> CreateFileAsync(PrPriceRequestFile file);
        Task<string> GenerateRequestNumberAsync();
        
        // Add more methods as needed (Update, Delete, List)
        Task<PagedResult<PrPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? status = null);
        Task<bool> UpdateStatusAsync(int requestId, string status, int updatedBy);
        Task<bool> UpdateItemPriceAsync(int itemId, decimal unitPrice, string currency, string? supplierName, int? leadTimeDays, string? priceRemark, int updatedBy);
    }
}
