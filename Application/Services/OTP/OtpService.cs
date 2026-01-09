using Domain.Interfaces;
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

        public string GenerateOTP(int orderId)
        {
            try
            {
                // Generate 4-digit OTP code
                var otpCode = new Random().Next(1000, 9999).ToString();

                // Store in memory cache with 5 minutes expiration
                string cacheKey = $"OTP_{orderId}";
                _cache.Set(cacheKey, otpCode, TimeSpan.FromMinutes(5));

                // Send notification via SignalR (async fire-and-forget)
                Task.Run(async () =>
                {
                    try
                    {
                        string message = $"Your OTP code for Purchase Order #{orderId} is: {otpCode}";
                        await _notificationService.SendNotificationAsync(message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending OTP notification");
                    }
                });

                _logger.LogInformation("OTP generated for order {OrderId}: {OTP}", orderId, otpCode);

                return otpCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for order {OrderId}", orderId);
                throw;
            }
        }

        public bool ValidateOTP(int orderId, string otp)
        {
            try
            {
                string cacheKey = $"OTP_{orderId}";
                
                if (_cache.TryGetValue(cacheKey, out string? storedOtp))
                {
                    if (storedOtp == otp)
                    {
                        // OTP is valid - remove it from cache
                        _cache.Remove(cacheKey);
                        _logger.LogInformation("OTP validated successfully for order {OrderId}", orderId);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Invalid OTP provided for order {OrderId}", orderId);
                    }
                }
                else
                {
                    _logger.LogWarning("OTP not found or expired for order {OrderId}", orderId);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating OTP for order {OrderId}", orderId);
                return false;
            }
        }

        public void ClearOTP(int orderId)
        {
            string cacheKey = $"OTP_{orderId}";
            _cache.Remove(cacheKey);
            _logger.LogInformation("OTP cleared for order {OrderId}", orderId);
        }
    }
}
