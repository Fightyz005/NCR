using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace NCRManagementSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("setup-demo")]
        [AllowAnonymous] // อนุญาตให้เข้าถึงได้โดยไม่ต้อง login
        public async Task<IActionResult> SetupDemo()
        {
            try
            {
                var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                // เรียกใช้ method ที่เพิ่มใน UserRepository
                var result = await userRepository.CreateDemoUsersIfNotExistAsync();

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Demo users created successfully",
                        users = new[] {
                    "demo.user (User role)",
                    "demo.qa (QA role)",
                    "demo.manager (Manager role)",
                    "demo.admin (Admin role)"
                }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to create demo users" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up demo users");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // เพิ่ม method ตรวจสอบฐานข้อมูล
        [HttpGet]
        [Route("check-database")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                var userRepository = HttpContext.RequestServices.GetRequiredService<IUserRepository>();

                // ตรวจสอบจำนวน users ทั้งหมด
                var totalUsers = await userRepository.GetTotalUsersAsync();

                // ตรวจสอบ demo users
                var demoUsers = new List<object>();
                var demoUsernames = new[] { "demo.user", "demo.qa", "demo.manager", "demo.admin" };

                foreach (var username in demoUsernames)
                {
                    var user = await userRepository.GetByUsernameAsync(username);
                    demoUsers.Add(new
                    {
                        username,
                        exists = user != null,
                        role = user?.Role,
                        isActive = user?.IsActive
                    });
                }

                return Json(new
                {
                    success = true,
                    totalUsers,
                    demoUsers,
                    message = "Database check completed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            try
            {
                var user = await _authService.ValidateUserAsync(model.Username, model.Password, model.Role);

                if (user == null)
                {
                    return Json(new { success = false, message = "ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง" });
                }

                // Create claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.GivenName, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("Department", user.Department ?? "")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                // Update last login
                await _authService.UpdateLastLoginAsync(user.UserId);

                _logger.LogInformation("User {Username} logged in successfully", model.Username);

                // Determine redirect URL based on role
                var redirectUrl = GetRedirectUrlByRole(user.Role, model.ReturnUrl);

                return Json(new { success = true, redirectUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในระบบ กรุณาลองใหม่อีกครั้ง" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("User logged out successfully");

                return Json(new { success = true, redirectUrl = Url.Action("Login", "Auth") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการออกจากระบบ" });
            }
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string GetRedirectUrlByRole(string role, string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return role switch
            {
                "Admin" => Url.Action("Index", "Dashboard") ?? "/",
                "Manager" => Url.Action("Index", "Manager") ?? "/",
                "QA" => Url.Action("Index", "QA") ?? "/",
                "User" => Url.Action("Index", "Supplier") ?? "/",
                _ => Url.Action("Index", "Dashboard") ?? "/"
            };
        }
    }
}