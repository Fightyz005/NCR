using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.ViewModels;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IPrPriceRequestService
    {
        Task<List<ExternalPrItemDto>> GetPrItemsAsync(string banfn);
        Task<int> CreateRequestAsync(PrPriceRequestCreateViewModel model, int userId, string? department);
        Task<PagedResult<PrPriceRequestDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, string? status = null);
        Task<PrPriceRequestDto?> GetDetailsAsync(int requestId);
        Task SendNotificationToPurchasingAsync(int requestId);
    }
}
