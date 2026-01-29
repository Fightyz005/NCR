using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.ViewModels
{
    public class NCRListViewModel
    {
        public List<NCRDto> NCRs { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search filters
        [Display(Name = "ค้นหา")]
        public string? SearchTerm { get; set; }

        [Display(Name = "สถานะ")]
        public string? Status { get; set; }

        [Display(Name = "เกรด")]
        public string? Grade { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [Display(Name = "วันที่เริ่มต้น")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "วันที่สิ้นสุด")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        // Lists for filters
        public List<SelectListItem> StatusOptions { get; set; } = new()
        {
            new SelectListItem { Value = "", Text = "ทุกสถานะ" },
            new SelectListItem { Value = "New", Text = "ใหม่" },
            new SelectListItem { Value = "Sent", Text = "ส่งแล้ว" },
            new SelectListItem { Value = "Replied", Text = "ตอบกลับแล้ว" },
            new SelectListItem { Value = "Approved", Text = "อนุมัติแล้ว" },
            new SelectListItem { Value = "Closed", Text = "ปิดงาน" }
        };

        public List<SelectListItem> GradeOptions { get; set; } = new()
        {
            new SelectListItem { Value = "", Text = "ทุกเกรด" },
            new SelectListItem { Value = "A", Text = "เกรด A" },
            new SelectListItem { Value = "B", Text = "เกรด B" },
            new SelectListItem { Value = "C", Text = "เกรด C" }
        };

        public List<SelectListItem> Suppliers { get; set; } = new();

        // Pagination properties
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class QAReviewViewModel
    {
        public NCRDto NCR { get; set; } = new();

        [Display(Name = "ส่งถึง")]
        public string SendTo { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกอีเมลผู้รับ")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        [Display(Name = "อีเมลผู้รับ")]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกวันครบกำหนดตอบกลับ")]
        [Display(Name = "วันครบกำหนดตอบกลับ")]
        [DataType(DataType.Date)]
        public DateTime ResponseDueDate { get; set; }

        [Display(Name = "ข้อความเพิ่มเติม")]
        [StringLength(1000)]
        public string? AdditionalMessage { get; set; }

        [Display(Name = "แนบเอกสารเพิ่มเติม")]
        public List<IFormFile> AdditionalFiles { get; set; } = new();

        public List<SelectListItem> SendToOptions { get; set; } = new()
        {
            new SelectListItem { Value = "supplier", Text = "Supplier" },
            new SelectListItem { Value = "production", Text = "ฝ่ายผลิต" },
            new SelectListItem { Value = "warehouse", Text = "ฝ่ายคลังสินค้า" }
        };

        public List<PendingTaskDto> PendingReviews { get; set; } = new();
    }

    public class SupplierResponseViewModel
    {
        public NCRDto NCR { get; set; } = new();

        [Required(ErrorMessage = "กรุณากรอกสาเหตุของปัญหา")]
        [Display(Name = "สาเหตุของปัญหา")]
        [StringLength(2000)]
        public string RootCause { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกวิธีการแก้ไข")]
        [Display(Name = "วิธีการแก้ไข")]
        [StringLength(2000)]
        public string CorrectiveAction { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกวิธีป้องกันไม่ให้เกิดซ้ำ")]
        [Display(Name = "วิธีป้องกันไม่ให้เกิดซ้ำ")]
        [StringLength(2000)]
        public string PreventiveAction { get; set; } = string.Empty;

        [Display(Name = "วันที่แก้ไขเสร็จสิ้น")]
        [DataType(DataType.Date)]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "ผู้รับผิดชอบ")]
        [StringLength(100)]
        public string? ResponsiblePerson { get; set; }

        [Display(Name = "แนบหลักฐานการแก้ไข")]
        public List<IFormFile> EvidenceFiles { get; set; } = new();

        public List<PendingTaskDto> PendingResponses { get; set; } = new();
    }

    public class ManagerApprovalViewModel
    {
        public NCRDto NCR { get; set; } = new();

        [Required(ErrorMessage = "กรุณาเลือกการตัดสินใจ")]
        [Display(Name = "การตัดสินใจ")]
        public string ApprovalDecision { get; set; } = string.Empty;

        [Display(Name = "ความเห็นของ Manager")]
        [StringLength(1000)]
        public string? ManagerComments { get; set; }

        [Display(Name = "วันที่อนุมัติ")]
        [DataType(DataType.Date)]
        public DateTime ApprovalDate { get; set; } = DateTime.Now;

        public List<SelectListItem> ApprovalOptions { get; set; } = new()
        {
            new SelectListItem { Value = "approve", Text = "อนุมัติปิด NCR" },
            new SelectListItem { Value = "reject", Text = "ไม่อนุมัติ (ส่งกลับแก้ไข)" }
        };

        public List<PendingTaskDto> PendingApprovals { get; set; } = new();
    }

    public class ReportViewModel
    {
        [Display(Name = "ประเภทรายงาน")]
        public string ReportType { get; set; } = string.Empty;

        [Display(Name = "วันที่เริ่มต้น")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "วันที่สิ้นสุด")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        public List<SelectListItem> ReportTypes { get; set; } = new()
        {
            new SelectListItem { Value = "MONTHLY_TREND", Text = "แนวโน้ม NCR รายเดือน" },
            new SelectListItem { Value = "SUPPLIER_PERFORMANCE", Text = "ประสิทธิภาพ Supplier" },
            new SelectListItem { Value = "PROBLEM_ANALYSIS", Text = "วิเคราะห์ปัญหา" }
        };

        public List<SelectListItem> Suppliers { get; set; } = new();

        // Report data
        public DashboardStatsDto Stats { get; set; } = new();
        public List<SupplierPerformanceDto> SupplierPerformance { get; set; } = new();
        public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
    }
}
