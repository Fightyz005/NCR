using NCRManagementSystem.Models.DTOs;

namespace NCRManagementSystem.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public List<NCRDto> RecentNCRs { get; set; } = new();
        public List<PendingTaskDto> PendingTasks { get; set; } = new();
        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
    }
}
