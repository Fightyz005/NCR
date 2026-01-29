using NCRManagementSystem.Models.Entities;

namespace NCRManagementSystem.Models.DTOs
{
    public class PrPriceRequestDto
    {
        public int RequestId { get; set; }
        public string PrNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string RequestByName { get; set; } = string.Empty;
        public string? VendorEmail { get; set; }
        public string Status { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int PricedItems { get; set; }
        public string StatusBadgeClass => Status == "Pending" ? "bg-warning" : (Status == "Completed" ? "bg-success" : "bg-secondary");
        public string StatusDisplay => Status == "Pending" ? "รอดำเนินการ" : Status;
        
        public List<PrPriceRequestItemDto> Items { get; set; } = new();
    }

    public class PrPriceRequestItemDto
    {
        public int ItemId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Remark { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Currency { get; set; }
        public string? SupplierName { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<PrPriceRequestFileDto> Files { get; set; } = new();
    }

    public class PrPriceRequestFileDto
    {
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
    }
}
