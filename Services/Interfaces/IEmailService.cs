namespace NCRManagementSystem.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendNCRNotificationAsync(string to, string subject, string body, List<string>? attachments = null);
        Task<bool> SendNCRStatusUpdateAsync(int ncrId, string newStatus, List<string> recipients);
        Task<bool> SendDueDateReminderAsync(int ncrId, List<string> recipients);
    }
}
