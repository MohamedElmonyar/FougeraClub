using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.OTP
{
    public interface IOtpService
    {
        string GenerateOTP(int orderId);
        bool ValidateOTP(int orderId, string otp);
        void ClearOTP(int orderId);
    }
}
