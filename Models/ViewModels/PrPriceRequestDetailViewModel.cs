using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Models.ViewModels
{
    public class PrPriceRequestDetailViewModel
    {
        public PrPriceRequestDto Request { get; set; } = new();
        public bool CanUpdatePrice { get; set; }
        public bool CanComplete { get; set; }
        public bool IsRequester { get; set; }
    }

    public class PrPriceUpdateViewModel
    {
        public PrPriceRequestDto Request { get; set; } = new();
        public List<PrPriceUpdateItemModel> Items { get; set; } = new();
    }

    public class PrPriceUpdateItemModel
    {
        public int ItemId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string Status { get; set; } = string.Empty;
        
        public decimal? UnitPrice { get; set; }
        public string Currency { get; set; } = "THB";
        public string? SupplierName { get; set; }
        public int? LeadTimeDays { get; set; }
        public string? PriceRemark { get; set; }
        
        public List<PrPriceRequestFileDto> Files { get; set; } = new();
    }
}
