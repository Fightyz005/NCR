using System.ComponentModel.DataAnnotations;

namespace NCRManagementSystem.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้")]
        [Display(Name = "ชื่อผู้ใช้")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกบทบาท")]
        [Display(Name = "บทบาท")]
        public string Role { get; set; } = string.Empty;

        [Display(Name = "จำรหัsผ่าน")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
