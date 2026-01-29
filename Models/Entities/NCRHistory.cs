using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class NCRHistory
    {
        public int HistoryId { get; set; }

        [Required]
        public int NCRId { get; set; }

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

        [Required]
        public int ActionBy { get; set; }

        [StringLength(1000)]
        public string? Comments { get; set; }

        // Navigation properties
        public NCR? NCR { get; set; }
        public User? ActionByUser { get; set; }
    }
}
