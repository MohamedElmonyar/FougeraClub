using Application.Services.Notifications;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services.OTP
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OtpService> _logger;

        public OtpService(
            IMemoryCache cache,
            INotificationService notificationService,
            ILogger<OtpService> logger)
        {
            _cache = cache;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<string> GenerateAndSendOtpAsync(int orderId)
        {
            try
            {
                // 1. Generate 4-digit Code
                var otpCode = new Random().Next(1000, 9999).ToString();

                // 2. Save to Memory Cache (Expires in 3 mins)
                string cacheKey = $"OTP_{orderId}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(3));

                // 3. Send via SignalR
                string message = $"Your verification code for Order #{orderId} is: {otpCode}";
                await _notificationService.SendNotificationAsync(message);

                _logger.LogInformation($"OTP Generated for Order {orderId}: {otpCode}");

                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP");
                throw;
            }
        }

        public bool VerifyOtp(int orderId, string userCode)
        {
            string cacheKey = $"OTP_{orderId}";
            if (_cache.TryGetValue(cacheKey, out string? storedCode))
            {
                if (storedCode == userCode)
                {
                    _cache.Remove(cacheKey); // Invalidate after use
                    return true;
                }
            }
            return false;
        }
    }
}
