namespace Receply.Application.Auth;

/// <summary>Delivers a plain-text email, e.g. a one-time signup code.</summary>
public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string body, CancellationToken cancellationToken = default);
}
