using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.Entities
{
    public class SystemSetting
    {
        public int SettingId { get; set; }

        [Required]
        [StringLength(50)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string SettingValue { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; }
    }

    // Enums
    public enum NCRStatus
    {
        New,
        Sent,
        Replied,
        Approved,
        Closed,
        Rejected
    }

    public enum NCRGrade
    {
        A, // ร้ายแรงมาก
        B, // ร้ายแรงปานกลาง
        C  // ร้ายแรงน้อย
    }

    public enum NCRPriority
    {
        Normal,
        Urgent
    }

    

    public enum FileCategory
    {
        General,
        Evidence,
        Response,
        Corrective
    }

    public enum CommentType
    {
        General,
        QA,
        Supplier,
        Manager
    }
}
