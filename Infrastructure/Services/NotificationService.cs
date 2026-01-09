using Domain.Interfaces;
using Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<Notifications> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<Notifications> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string message)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveOTP", new
                {
                    title = "OTP Code",
                    message = message,
                    otp = message.Split(':').LastOrDefault()?.Trim() ?? ""
                });

                _logger.LogInformation("Notification sent via SignalR: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification via SignalR");
                throw;
            }
        }
    }
}
