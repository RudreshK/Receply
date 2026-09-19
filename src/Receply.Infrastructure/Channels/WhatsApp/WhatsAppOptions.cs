namespace Receply.Infrastructure.Channels.WhatsApp;

public class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    /// <summary>Meta Graph API base URL, e.g. https://graph.facebook.com</summary>
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";
    public string ApiVersion { get; set; } = "v20.0";

    /// <summary>Permanent or system-user access token for the WhatsApp Business Account.</summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>App secret used to verify the X-Hub-Signature-256 header on inbound webhooks.</summary>
    public string AppSecret { get; set; } = default!;

    /// <summary>Token you choose and register with Meta for the webhook verification handshake.</summary>
    public string WebhookVerifyToken { get; set; } = default!;
}
