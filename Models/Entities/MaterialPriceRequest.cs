// Models/Entities/MaterialPriceRequest.cs
using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class MaterialPriceRequest
    {
        public int RequestId { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestNumber { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public int RequestBy { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Cancelled

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime? CompletedDate { get; set; }
        public int? CompletedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation properties
        public User? RequestByUser { get; set; }
        public User? CompletedByUser { get; set; }
        public List<MaterialPriceRequestItem> Items { get; set; } = new();
        public List<MaterialPriceRequestHistory> History { get; set; } = new();
    }

    public class MaterialPriceRequestItem
    {
        public int ItemId { get; set; }
        public int RequestId { get; set; }

        [Required]
        [StringLength(10)]
        public string Plant { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        public decimal? Quantity { get; set; }

        [StringLength(20)]
        public string? Unit { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        // ข้อมูลราคาจากฝ่ายจัดซื้อ
        public decimal? UnitPrice { get; set; }

        [StringLength(10)]
        public string? Currency { get; set; } = "THB";

        [StringLength(200)]
        public string? SupplierName { get; set; }

        public int? LeadTimeDays { get; set; }

        [StringLength(500)]
        public string? PriceRemark { get; set; }

        public DateTime? PriceUpdatedDate { get; set; }
        public int? PriceUpdatedBy { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Priced

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Calculated property
        public bool IsOverdue => Status == "Pending" && (DateTime.Now - CreatedDate).TotalHours >= 3;

        // Navigation properties
        public MaterialPriceRequest? Request { get; set; }
        public User? PriceUpdatedByUser { get; set; }
        public List<MaterialPriceRequestFile> Files { get; set; } = new();
    }

    public class MaterialPriceRequestFile
    {
        public int FileId { get; set; }
        public int ItemId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; } = DateTime.Now;
        public int UploadedBy { get; set; }

        // Navigation
        public MaterialPriceRequestItem? Item { get; set; }
        public User? UploadedByUser { get; set; }
    }

    public class MaterialPriceRequestHistory
    {
        public int HistoryId { get; set; }
        public int RequestId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(20)]
        public string? OldStatus { get; set; }

        [StringLength(20)]
        public string? NewStatus { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.Now;
        public int ActionBy { get; set; }

        // Navigation
        public MaterialPriceRequest? Request { get; set; }
        public User? ActionByUser { get; set; }
    }

    public class MaterialPriceRequestNotification
    {
        public int NotificationId { get; set; }
        public int RequestId { get; set; }
        public string NotificationType { get; set; } = string.Empty; // Email, Line
        public string RecipientType { get; set; } = string.Empty; // Production, Purchasing
        public DateTime? SentDate { get; set; }
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
        public int RetryCount { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}