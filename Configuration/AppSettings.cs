using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Configuration
{
    public class AppSettings
    {
        [Required]
        public string JwtSecret { get; set; } = string.Empty;

        public int JwtExpireHours { get; set; } = 8;

        public int DefaultPageSize { get; set; } = 10;

        public int MaxFileUploadSizeMB { get; set; } = 10;

        public List<string> AllowedFileExtensions { get; set; } = new();

        public string NCRNumberFormat { get; set; } = "NCR-{YYYY}-{###}";

        public DefaultDueDays DefaultDueDays { get; set; } = new();
    }

    public class DefaultDueDays
    {
        public int GradeA { get; set; } = 2;
        public int GradeB { get; set; } = 5;
        public int GradeC { get; set; } = 7;
    }

    

    public class EmailSettings
    {
        [Required]
        public string SmtpServer { get; set; } = string.Empty;

        public int SmtpPort { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string FromEmail { get; set; } = string.Empty;
    }

    public class FileUploadSettings
    {
        public string UploadPath { get; set; } = "wwwroot/uploads/ncr-files";
        public int MaxFileSizeMB { get; set; } = 10;
        public List<string> AllowedExtensions { get; set; } = new();
        public int ImageMaxWidth { get; set; } = 1920;
        public int ImageMaxHeight { get; set; } = 1080;
        public int ImageQuality { get; set; } = 85;
    }

    public class SecuritySettings
    {
        public bool EnableBruteForceProtection { get; set; } = true;
        public int MaxLoginAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public bool RequireStrongPassword { get; set; } = true;
        public int PasswordMinLength { get; set; } = 6;
        public int SessionTimeoutMinutes { get; set; } = 30;
        public bool EnableAuditLog { get; set; } = true;
    }

    public class NotificationSettings
    {
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableBrowserNotifications { get; set; } = true;
        public bool NotifyOnNCRCreated { get; set; } = true;
        public bool NotifyOnNCRStatusChanged { get; set; } = true;
        public bool NotifyOnDueDateApproaching { get; set; } = true;
        public int DueDateWarningDays { get; set; } = 1;
    }
}