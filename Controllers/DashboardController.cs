using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCRManagementSystem.Models.ViewModels;
using NCRManagementSystem.Services.Interfaces;
using System.Security.Claims;

namespace NCRManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();
                var userName = GetCurrentUserName();

                var stats = await _dashboardService.GetDashboardStatsAsync(userId, userRole);
                var recentNCRs = await _dashboardService.GetRecentNCRsAsync(10);
                var pendingTasks = await _dashboardService.GetPendingTasksAsync(userRole, userId);

                var viewModel = new DashboardViewModel
                {
                    Stats = stats,
                    RecentNCRs = recentNCRs,
                    PendingTasks = pendingTasks,
                    UserRole = userRole,
                    UserName = userName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData(string chartType = "grade")
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                switch (chartType.ToLower())
                {
                    case "grade":
                        var gradeData = await _dashboardService.GetGradeChartDataAsync(userId, userRole);
                        return Json(new { success = true, data = gradeData });

                    case "trend":
                        var trendData = await _dashboardService.GetTrendChartDataAsync(6); // Last 6 months
                        return Json(new { success = true, data = trendData });

                    case "supplier":
                        var supplierData = await _dashboardService.GetTopSuppliersDataAsync(5);
                        return Json(new { success = true, data = supplierData });

                    default:
                        return Json(new { success = false, message = "Invalid chart type" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chart data for type {ChartType}", chartType);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการโหลดข้อมูลกราฟ" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            try
            {
                var userId = GetCurrentUserId();
                var userRole = GetCurrentUserRole();

                var notifications = await _dashboardService.GetNotificationsAsync(userId, userRole);

                return Json(new { success = true, data = notifications });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", GetCurrentUserId());
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการโหลดการแจ้งเตือน" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationAsRead(int notificationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _dashboardService.MarkNotificationAsReadAsync(notificationId, userId);

                if (result)
                {
                    return Json(new { success = true, message = "อัพเดตการแจ้งเตือนเรียบร้อย" });
                }
                else
                {
                    return Json(new { success = false, message = "ไม่สามารถอัพเดตการแจ้งเตือนได้" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return Json(new { success = false, message = "เกิดข้อผิดพลาดในการอัพเดตการแจ้งเตือน" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        }

        private string GetCurrentUserName()
        {
            return User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        }
    }
}