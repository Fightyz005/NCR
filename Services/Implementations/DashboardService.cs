using NCRManagementSystem.Models.DTOs;
using NCRManagementSystem.Repositories.Interfaces;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly INCRRepository _ncrRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(INCRRepository ncrRepository, ILogger<DashboardService> logger)
        {
            _ncrRepository = ncrRepository;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(int? userId = null, string? userRole = null)
        {
            try
            {
                return await _ncrRepository.GetDashboardStatsAsync(userId, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return new DashboardStatsDto();
            }
        }

        public async Task<List<NCRDto>> GetRecentNCRsAsync(int count = 10)
        {
            try
            {
                return await _ncrRepository.GetRecentAsync(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent NCRs");
                return new List<NCRDto>();
            }
        }

        public async Task<List<PendingTaskDto>> GetPendingTasksAsync(string userRole, int? userId = null)
        {
            try
            {
                return await _ncrRepository.GetPendingByRoleAsync(userRole, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending tasks for role {UserRole}", userRole);
                return new List<PendingTaskDto>();
            }
        }

        public async Task<object> GetGradeChartDataAsync(int? userId = null, string? userRole = null)
        {
            try
            {
                var stats = await GetDashboardStatsAsync(userId, userRole);
                return new
                {
                    labels = new[] { "เกรด A", "เกรด B", "เกรด C" },
                    data = new[] { stats.GradeACount, stats.GradeBCount, stats.GradeCCount }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting grade chart data");
                return new { labels = new string[0], data = new int[0] };
            }
        }

        public async Task<object> GetTrendChartDataAsync(int months = 6)
        {
            try
            {
                // This would require additional database query
                // For now, return sample data
                await Task.CompletedTask;
                return new
                {
                    labels = new[] { "ม.ค.", "ก.พ.", "มี.ค.", "เม.ย.", "พ.ค.", "มิ.ย." },
                    datasets = new[]
                    {
                        new { label = "NCR ใหม่", data = new[] { 12, 19, 15, 18, 15, 12 } },
                        new { label = "NCR ปิด", data = new[] { 10, 15, 13, 17, 14, 11 } }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trend chart data");
                return new { labels = new string[0], datasets = new object[0] };
            }
        }

        public async Task<object> GetTopSuppliersDataAsync(int count = 5)
        {
            try
            {
                // This would require additional database query
                // For now, return sample data
                await Task.CompletedTask;
                return new[]
                {
                    new { name = "ABC Supplier", count = 28 },
                    new { name = "XYZ Company", count = 22 },
                    new { name = "DEF Industries", count = 18 },
                    new { name = "GHI Corp", count = 15 },
                    new { name = "JKL Ltd", count = 12 }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top suppliers data");
                return new object[0];
            }
        }

        public async Task<List<object>> GetNotificationsAsync(int userId, string userRole)
        {
            try
            {
                var pendingTasks = await GetPendingTasksAsync(userRole, userId);
                var notifications = new List<object>();

                foreach (var task in pendingTasks.Take(5))
                {
                    var timeAgo = GetTimeAgo(DateTime.Now - (task.DueDate ?? DateTime.Now));
                    notifications.Add(new
                    {
                        message = $"NCR {task.NCRNumber} - {task.ProductName}",
                        timeAgo = timeAgo,
                        url = $"/NCR/Details/{task.NCRId}",
                        type = task.IsOverdue ? "overdue" : "pending"
                    });
                }

                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                return new List<object>();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId)
        {
            try
            {
                // Implementation would depend on how notifications are stored
                // For now, just return true
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
                return false;
            }
        }

        private static string GetTimeAgo(TimeSpan timeSpan)
        {
            if (timeSpan.Days > 0)
                return $"{timeSpan.Days} วันที่แล้ว";
            if (timeSpan.Hours > 0)
                return $"{timeSpan.Hours} ชั่วโมงที่แล้ว";
            if (timeSpan.Minutes > 0)
                return $"{timeSpan.Minutes} นาทีที่แล้ว";
            return "เมื่อสักครู่";
        }
    }
}
