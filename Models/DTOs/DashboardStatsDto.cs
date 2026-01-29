namespace NCRManagementSystem.Models.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalNCRs { get; set; }
        public int OpenNCRs { get; set; }
        public int ClosedNCRs { get; set; }
        public int MonthlyNCRs { get; set; }
        public int GradeACount { get; set; }
        public int GradeBCount { get; set; }
        public int GradeCCount { get; set; }
        public List<NCRDto> RecentNCRs { get; set; } = new();
        public List<PendingTaskDto> PendingTasks { get; set; } = new();
    }
}
