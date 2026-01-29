using Microsoft.AspNetCore.Mvc.Rendering;
using NCRManagementSystem.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.ViewModels
{
    public class NCRViewModel
    {
        public int NCRId { get; set; }

        [Display(Name = "เลข NCR")]
        public string NCRNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
        [Display(Name = "ชื่อสินค้า")]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "ITEM Code")]
        [StringLength(50)]
        public string? ItemCode { get; set; }

        [Required(ErrorMessage = "กรุณาเลือก Supplier")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Display(Name = "Lot No.")]
        [StringLength(50)]
        public string? LotNumber { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกเกรดของปัญหา")]
        [Display(Name = "เกรดของปัญหา")]
        public string Grade { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกความเร่งด่วน")]
        [Display(Name = "ความเร่งด่วน")]
        public string Priority { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรายละเอียดของปัญหา")]
        [Display(Name = "รายละเอียดของปัญหา")]
        [StringLength(2000)]
        public string ProblemDescription { get; set; } = string.Empty;

        [Display(Name = "สถานะ")]
        public string Status { get; set; } = "New";

        [Display(Name = "วันที่แจ้ง")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "วันครบกำหนด")]
        public DateTime? DueDate { get; set; }

        // ⭐ เพิ่มฟิลด์เหล่านี้ที่ขาดหายไป
        [Display(Name = "สาเหตุของปัญหา (Root Cause)")]
        [StringLength(1000)]
        public string RootCause { get; set; } = string.Empty;

        [Display(Name = "การแก้ไขเฉพาะหน้า (Corrective Action)")]
        [StringLength(1000)]
        public string CorrectiveAction { get; set; } = string.Empty;

        [Display(Name = "การป้องกันในอนาคต (Preventive Action)")]
        [StringLength(1000)]
        public string PreventiveAction { get; set; } = string.Empty;

        [Display(Name = "ผู้รับผิดชอบ")]
        [StringLength(100)]
        public string ResponsiblePerson { get; set; } = string.Empty;

        [Display(Name = "ความคิดเห็น QA")]
        [StringLength(1000)]
        public string QAComments { get; set; } = string.Empty;

        [Display(Name = "ความคิดเห็น Manager")]
        [StringLength(1000)]
        public string ManagerComments { get; set; } = string.Empty;

        // For file uploads
        [Display(Name = "แนบไฟล์หลักฐาน")]
        public List<IFormFile> Files { get; set; } = new();

        // Lists for dropdowns
        public List<SelectListItem> Suppliers { get; set; } = new();
        public List<SelectListItem> Grades { get; set; } = new()
        {
            new SelectListItem { Value = "A", Text = "A - ร้ายแรงมาก" },
            new SelectListItem { Value = "B", Text = "B - ร้ายแรงปานกลาง" },
            new SelectListItem { Value = "C", Text = "C - ร้ายแรงน้อย" }
        };
        public List<SelectListItem> Priorities { get; set; } = new()
        {
            new SelectListItem { Value = "Normal", Text = "ปกติ" },
            new SelectListItem { Value = "Urgent", Text = "ด่วนมาก" }
        };

        // Additional properties for display
        public string? SupplierName { get; set; }
        public string? CreatedByName { get; set; }
        public List<NCRFileDto> ExistingFiles { get; set; } = new();
        public List<NCRHistoryDto> History { get; set; } = new();
        public List<NCRCommentDto> Comments { get; set; } = new();
    }
}
