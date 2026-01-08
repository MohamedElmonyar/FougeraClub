using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.OTP
{
    public interface IOtpService
    {
        Task<string> GenerateAndSendOtpAsync(int orderId);
        bool VerifyOtp(int orderId, string code);
    }
}
