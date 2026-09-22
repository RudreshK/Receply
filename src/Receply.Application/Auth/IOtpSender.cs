namespace Receply.Application.Auth;

/// <summary>Delivers a one-time login code to a staff member's phone.</summary>
public interface IOtpSender
{
    Task SendAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
}
