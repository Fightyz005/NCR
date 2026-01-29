using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class NCRComment
    {
        public int CommentId { get; set; }

        [Required]
        public int NCRId { get; set; }

        [Required]
        [StringLength(1000)]
        public string CommentText { get; set; } = string.Empty;

        [StringLength(20)]
        public string CommentType { get; set; } = "General";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int CreatedBy { get; set; }

        public bool IsResolved { get; set; } = false;

        public int? ParentCommentId { get; set; }

        // Navigation properties
        public NCR? NCR { get; set; }
        public User? CreatedByUser { get; set; }
        public NCRComment? ParentComment { get; set; }
        public List<NCRComment> Replies { get; set; } = new();
    }
}
