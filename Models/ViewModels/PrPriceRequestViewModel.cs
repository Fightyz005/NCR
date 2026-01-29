using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.ViewModels
{
    public class PrPriceRequestListViewModel
    {
        public List<PrPriceRequestDto> Requests { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class PrPriceRequestCreateViewModel
    {
        [Required(ErrorMessage = "กรุณาระบุเลขที่ PR")]
        [Display(Name = "เลขที่ PR (BANFN)")]
        public string PrNumber { get; set; } = string.Empty;

        [Display(Name = "อีเมล์ Vendor")]
        [EmailAddress(ErrorMessage = "กรุณากรอกอีเมล์ให้ถูกต้อง")]
        public string? VendorEmail { get; set; }

        [Display(Name = "หมายเหตุ")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        public List<PrPriceRequestItemInputModel> Items { get; set; } = new();
    }

    public class PrPriceRequestItemInputModel
    {
        public string? Plant { get; set; }
        public string? MaterialCode { get; set; }
        public string? MaterialName { get; set; }
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Remark { get; set; }
        public List<IFormFile> Files { get; set; } = new();
    }



}
