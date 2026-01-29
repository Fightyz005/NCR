using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class NCRFile
    {
        public int FileId { get; set; }

        [Required]
        public int NCRId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        [Required]
        [StringLength(50)]
        public string FileType { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; } = DateTime.Now;

        [Required]
        public int UploadedBy { get; set; }

        [StringLength(50)]
        public string FileCategory { get; set; } = "General";

        // Navigation properties
        public NCR? NCR { get; set; }
        public User? UploadedByUser { get; set; }
    }
}
