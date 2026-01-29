// Models/ViewModels/MaterialPriceRequestViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.ViewModels
{
    public class MaterialPriceRequestListViewModel
    {
        public List<MaterialPriceRequestDto> Requests { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filters
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public List<SelectListItem> StatusOptions { get; set; } = new()
        {
            new SelectListItem { Value = "", Text = "ทุกสถานะ" },
            new SelectListItem { Value = "Pending", Text = "รอดำเนินการ" },
            new SelectListItem { Value = "InProgress", Text = "กำลังดำเนินการ" },
            new SelectListItem { Value = "Completed", Text = "เสร็จสิ้น" },
            new SelectListItem { Value = "Cancelled", Text = "ยกเลิก" }
        };

        // Pagination
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Stats
        public MaterialPriceRequestStatsDto Stats { get; set; } = new();
    }

    public class MaterialPriceRequestCreateViewModel
    {
        public string RequestNumber { get; set; } = string.Empty;

        [Display(Name = "หมายเหตุ")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        public List<MaterialPriceRequestItemInputModel> Items { get; set; } = new();

        public List<SelectListItem> PlantOptions { get; set; } = new()
        {
            new SelectListItem { Value = "1000", Text = "KP01" },
            new SelectListItem { Value = "2000", Text = "KB01" },
            new SelectListItem { Value = "3000", Text = "KY01" }
        };
    }

    public class MaterialPriceRequestItemInputModel
    {
        public int? ItemId { get; set; }

        [Required(ErrorMessage = "กรุณาเลือก Plant")]
        [Display(Name = "Plant")]
        public string Plant { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสวัตถุดิบ")]
        [Display(Name = "รหัสวัตถุดิบ")]
        [StringLength(50)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อวัตถุดิบ")]
        [Display(Name = "ชื่อวัตถุดิบ")]
        [StringLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        [Display(Name = "จำนวน")]
        public decimal? Quantity { get; set; }

        [Display(Name = "หน่วย")]
        [StringLength(20)]
        public string? Unit { get; set; }

        [Display(Name = "หมายเหตุ")]
        [StringLength(500)]
        public string? Remark { get; set; }

        [Display(Name = "รูปภาพ")]
        public List<IFormFile> Files { get; set; } = new();

        // For existing files when editing
        public List<MaterialPriceRequestFileDto> ExistingFiles { get; set; } = new();
    }

    public class MaterialPriceRequestDetailViewModel
    {
        public MaterialPriceRequestDto Request { get; set; } = new();
        public bool CanUpdatePrice { get; set; }
        public bool CanComplete { get; set; }
        public bool IsRequester { get; set; }
    }

    public class MaterialPriceUpdateViewModel
    {
        public MaterialPriceRequestDto Request { get; set; } = new();

        public List<PriceUpdateItemModel> Items { get; set; } = new();

        public List<SelectListItem> CurrencyOptions { get; set; } = new()
        {
            new SelectListItem { Value = "THB", Text = "THB - บาท" },
            new SelectListItem { Value = "USD", Text = "USD - ดอลลาร์สหรัฐ" },
            new SelectListItem { Value = "EUR", Text = "EUR - ยูโร" },
            new SelectListItem { Value = "JPY", Text = "JPY - เยน" },
            new SelectListItem { Value = "CNY", Text = "CNY - หยวน" }
        };
    }

    public class PriceUpdateItemModel
    {
        public int ItemId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string Status { get; set; } = string.Empty;

        [Display(Name = "ราคาต่อหน่วย")]
        [Range(0.01, double.MaxValue, ErrorMessage = "กรุณากรอกราคาที่มากกว่า 0")]
        public decimal? UnitPrice { get; set; }

        [Display(Name = "สกุลเงิน")]
        public string Currency { get; set; } = "THB";

        [Display(Name = "ชื่อ Supplier")]
        [StringLength(200)]
        public string? SupplierName { get; set; }

        [Display(Name = "Lead Time (วัน)")]
        public int? LeadTimeDays { get; set; }

        [Display(Name = "หมายเหตุ")]
        [StringLength(500)]
        public string? PriceRemark { get; set; }

        public List<MaterialPriceRequestFileDto> Files { get; set; } = new();
    }
}