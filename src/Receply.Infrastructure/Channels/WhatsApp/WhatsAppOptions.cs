namespace Receply.Infrastructure.Channels.WhatsApp;

public class WhatsAppOptions
{
    public const string SectionName = "WhatsApp";

    /// <summary>Meta Graph API base URL, e.g. https://graph.facebook.com</summary>
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";
    public string ApiVersion { get; set; } = "v25.0";

    /// <summary>Permanent or system-user access token for the WhatsApp Business Account.</summary>
    public string AccessToken { get; set; } = default!;

    /// <summary>App secret used to verify the X-Hub-Signature-256 header on inbound webhooks.</summary>
    public string AppSecret { get; set; } = default!;

    /// <summary>Token you choose and register with Meta for the webhook verification handshake.</summary>
    public string WebhookVerifyToken { get; set; } = default!;

    /// <summary>
    /// The WhatsApp phone_number_id to bootstrap a demo ChannelAccount for on first run (see
    /// DbSeeder). Not used at request time - inbound messages are matched by the ChannelAccount
    /// rows already in the database, not by this config value.
    /// </summary>
    public string? DefaultPhoneNumberId { get; set; }
}
