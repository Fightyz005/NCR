using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class PrPriceRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        [StringLength(20)]
        public string PrNumber { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Required]
        public int RequestBy { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? VendorEmail { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime? CompletedDate { get; set; }
        public int? CompletedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }

        // Navigation
        public User? RequestByUser { get; set; }
        public List<PrPriceRequestItem> Items { get; set; } = new();
    }

    public class PrPriceRequestItem
    {
        [Key]
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

        // Price info
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
        public string Status { get; set; } = "Pending";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation
        public PrPriceRequest? Request { get; set; }
        public List<PrPriceRequestFile> Files { get; set; } = new();
    }

    public class PrPriceRequestFile
    {
        [Key]
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
        public PrPriceRequestItem? Item { get; set; }
    }
}
