// Services/Interfaces/ILineNotificationService.cs
namespace NCRManagementSystem.Services.Interfaces
{
    public interface ILineNotificationService
    {
        Task<bool> SendMessageAsync(string message, string groupType);
        Task<bool> SendMessageToTokenAsync(string message, string accessToken);
    }
}