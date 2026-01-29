using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync(int? userId = null, string? userRole = null);
        Task<List<NCRDto>> GetRecentNCRsAsync(int count = 10);
        Task<List<PendingTaskDto>> GetPendingTasksAsync(string userRole, int? userId = null);
        Task<object> GetGradeChartDataAsync(int? userId = null, string? userRole = null);
        Task<object> GetTrendChartDataAsync(int months = 6);
        Task<object> GetTopSuppliersDataAsync(int count = 5);
        Task<List<object>> GetNotificationsAsync(int userId, string userRole);
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId);
    }
}
