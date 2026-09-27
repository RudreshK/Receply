namespace Receply.Infrastructure.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;

    /// <summary>SMTP login - for Gmail this is the full mailbox address.</summary>
    public string Username { get; set; } = default!;

    /// <summary>For Gmail this must be an App Password, not the account's regular password.</summary>
    public string Password { get; set; } = default!;

    public string FromAddress { get; set; } = default!;
    public string FromName { get; set; } = "Receply";
}
