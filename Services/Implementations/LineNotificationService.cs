// Services/Implementations/LineNotificationService.cs
using System.Net.Http.Headers;
using System.Text;
using NCRManagementSystem.Services.Interfaces;

namespace NCRManagementSystem.Services.Implementations
{
    public class LineNotificationService : ILineNotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<LineNotificationService> _logger;
        private readonly HttpClient _httpClient;

        public LineNotificationService(IConfiguration configuration, ILogger<LineNotificationService> logger, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<bool> SendMessageAsync(string message, string groupType)
        {
            try
            {
                var accessToken = groupType switch
                {
                    "Purchasing" => _configuration["LineNotify:PurchasingToken"],
                    "Production" => _configuration["LineNotify:ProductionToken"],
                    _ => _configuration["LineNotify:DefaultToken"]
                };

                if (string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogWarning("Line Notify token not configured for group: {GroupType}", groupType);
                    return false;
                }

                return await SendMessageToTokenAsync(message, accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Line notification to {GroupType}", groupType);
                return false;
            }
        }

        public async Task<bool> SendMessageToTokenAsync(string message, string accessToken)
        {
            try
            {
                var url = "https://notify-api.line.me/api/notify";

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", message)
                });

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Line notification sent successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send Line notification: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Line notification");
                return false;
            }
        }
    }
}