// Models/DTOs/MaterialPriceRequestDto.cs
namespace NCRManagementSystem.Models.DTOs
{
    public class MaterialPriceRequestDto
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public int RequestBy { get; set; }
        public string RequestByName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? VendorEmail { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? CompletedByName { get; set; }
        public int TotalItems { get; set; }
        public int PricedItems { get; set; }
        public int PendingItems => TotalItems - PricedItems;
        public bool HasOverdueItems { get; set; }

        public string StatusDisplay => Status switch
        {
            "Pending" => "รอดำเนินการ",
            "InProgress" => "กำลังดำเนินการ",
            "Completed" => "เสร็จสิ้น",
            "Cancelled" => "ยกเลิก",
            _ => Status
        };

        public string StatusBadgeClass => Status switch
        {
            "Pending" => "bg-warning",
            "InProgress" => "bg-info",
            "Completed" => "bg-success",
            "Cancelled" => "bg-secondary",
            _ => "bg-secondary"
        };

        public List<MaterialPriceRequestItemDto> Items { get; set; } = new();
        public List<MaterialPriceRequestHistoryDto> History { get; set; } = new();
    }

    public class MaterialPriceRequestItemDto
    {
        public int ItemId { get; set; }
        public int RequestId { get; set; }
        public string Plant { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Remark { get; set; }

        // Price info
        public decimal? UnitPrice { get; set; }
        public string? Currency { get; set; }
        public string? SupplierName { get; set; }
        public int? LeadTimeDays { get; set; }
        public string? PriceRemark { get; set; }
        public DateTime? PriceUpdatedDate { get; set; }
        public string? PriceUpdatedByName { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public bool IsOverdue { get; set; }
        public int HoursPending => (int)(DateTime.Now - CreatedDate).TotalHours;

        public string StatusDisplay => Status == "Pending" ? "ยังไม่ได้ราคา" : "ได้ราคาแล้ว";
        public string StatusBadgeClass => Status == "Pending" ? (IsOverdue ? "bg-danger" : "bg-warning") : "bg-success";

        public string PriceDisplay => UnitPrice.HasValue ? $"{UnitPrice:N2} {Currency}" : "-";

        public List<MaterialPriceRequestFileDto> Files { get; set; } = new();
    }

    public class MaterialPriceRequestFileDto
    {
        public int FileId { get; set; }
        public int ItemId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedDate { get; set; }
        public string UploadedByName { get; set; } = string.Empty;

        public string FileSizeFormatted
        {
            get
            {
                string[] sizes = { "B", "KB", "MB", "GB" };
                double len = FileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public bool IsImage => FileType.StartsWith("image/");
    }

    public class MaterialPriceRequestHistoryDto
    {
        public int HistoryId { get; set; }
        public int RequestId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public DateTime ActionDate { get; set; }
        public string ActionByName { get; set; } = string.Empty;
    }

    public class MaterialPriceRequestStatsDto
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int MonthlyRequests { get; set; }
        public int OverdueItems { get; set; }
    }

    public class UpdatePriceDto
    {
        public int ItemId { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "THB";
        public string? SupplierName { get; set; }
        public int? LeadTimeDays { get; set; }
        public string? PriceRemark { get; set; }
    }
}