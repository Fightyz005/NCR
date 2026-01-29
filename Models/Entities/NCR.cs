using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class NCR
    {
        public int NCRId { get; set; }

        [Required]
        [StringLength(20)]
        public string NCRNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ItemCode { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [StringLength(50)]
        public string? LotNumber { get; set; }

        [Required]
        [StringLength(1)]
        public string Grade { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Priority { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string ProblemDescription { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "New";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int CreatedBy { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? QAReviewedDate { get; set; }

        public int? QAReviewedBy { get; set; }

        [StringLength(1000)]
        public string? QAComments { get; set; }

        public DateTime? SupplierResponseDate { get; set; }

        public int? SupplierResponseBy { get; set; }

        [StringLength(2000)]
        public string? RootCause { get; set; }

        [StringLength(2000)]
        public string? CorrectiveAction { get; set; }

        [StringLength(2000)]
        public string? PreventiveAction { get; set; }

        public DateTime? CompletionDate { get; set; }

        [StringLength(100)]
        public string? ResponsiblePerson { get; set; }

        public DateTime? ManagerApprovedDate { get; set; }

        public int? ManagerApprovedBy { get; set; }

        [StringLength(1000)]
        public string? ManagerComments { get; set; }

        public DateTime? ClosedDate { get; set; }

        public int? ClosedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public int? UpdatedBy { get; set; }

        // Navigation properties
        public Supplier? Supplier { get; set; }
        public User? CreatedByUser { get; set; }
        public User? QAReviewedByUser { get; set; }
        public User? ManagerApprovedByUser { get; set; }
        public User? ClosedByUser { get; set; }
        public List<NCRFile> Files { get; set; } = new();
        public List<NCRHistory> History { get; set; } = new();
        public List<NCRComment> Comments { get; set; } = new();
    }
}
