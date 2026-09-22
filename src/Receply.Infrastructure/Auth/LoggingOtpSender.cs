using Microsoft.Extensions.Logging;
using Receply.Application.Auth;

namespace Receply.Infrastructure.Auth;

/// <summary>
/// Dev-mode stand-in for OTP delivery: logs the code instead of sending it. No SMS gateway or
/// Meta-approved WhatsApp "Authentication" template is configured yet - replace this with a real
/// sender once one is.
/// </summary>
public class LoggingOtpSender(ILogger<LoggingOtpSender> logger) : IOtpSender
{
    public Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("OTP for {PhoneNumber}: {Code} (no OTP gateway configured - logging only)", phoneNumber, code);
        return Task.CompletedTask;
    }
}
