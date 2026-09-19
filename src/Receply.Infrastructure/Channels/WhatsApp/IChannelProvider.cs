using Receply.Domain.Channels;

namespace Receply.Infrastructure.Channels.WhatsApp;

public record InboundChannelMessage(string FromExternalId, string CustomerPhoneNumber, string Body, DateTimeOffset ReceivedAtUtc);

/// <summary>
/// Abstraction over a messaging channel provider (WhatsApp Cloud API today; SMS/Instagram later
/// implement the same interface so Conversations/AI code never depends on a specific provider).
/// </summary>
public interface IChannelProvider
{
    ChannelType Type { get; }

    Task SendMessageAsync(ChannelAccount account, string toPhoneNumber, string body, CancellationToken cancellationToken = default);

    /// <summary>Parses a provider-specific webhook payload into normalized inbound messages.</summary>
    IReadOnlyCollection<InboundChannelMessage> ParseInboundWebhook(string rawPayload);

    /// <summary>Verifies the provider's signature/handshake so inbound webhooks can't be spoofed.</summary>
    bool VerifyWebhookSignature(string rawPayload, string? signatureHeader);
}
