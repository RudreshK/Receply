using Receply.Domain.Channels;

namespace Receply.Application.Channels;

public record InboundChannelMessage(
    string FromExternalId,
    string CustomerPhoneNumber,
    string Body,
    DateTimeOffset ReceivedAtUtc,
    string? SenderDisplayName = null,
    string? ExternalMessageId = null);

/// <summary>A delivery status update for a message we previously sent (WhatsApp: sent/delivered/read).</summary>
public record InboundStatusUpdate(string FromExternalId, string ExternalMessageId, string Status, DateTimeOffset UpdatedAtUtc);

/// <summary>
/// Abstraction over a messaging channel provider (WhatsApp Cloud API today; SMS/Instagram later
/// implement the same interface so Conversations/AI code never depends on a specific provider).
/// Lives in Application (not Infrastructure) because the AI reply pipeline needs to send messages
/// back to the customer, and Application can't depend on Infrastructure.
/// </summary>
public interface IChannelProvider
{
    ChannelType Type { get; }

    /// <summary>Sends a text message and returns the provider's id for it, used to match later
    /// delivery-status webhooks back to the message we saved.</summary>
    Task<string> SendMessageAsync(ChannelAccount account, string toPhoneNumber, string body, CancellationToken cancellationToken = default);

    /// <summary>Marks an inbound message as read and asks the provider to show a "typing…" indicator
    /// to the customer while we prepare a reply - best-effort, never throws.</summary>
    Task ShowTypingIndicatorAsync(ChannelAccount account, string inboundExternalMessageId, CancellationToken cancellationToken = default);

    /// <summary>Parses a provider-specific webhook payload into normalized inbound messages.</summary>
    IReadOnlyCollection<InboundChannelMessage> ParseInboundWebhook(string rawPayload);

    /// <summary>Parses a provider-specific webhook payload into delivery-status updates for messages we sent.</summary>
    IReadOnlyCollection<InboundStatusUpdate> ParseStatusUpdates(string rawPayload);

    /// <summary>Verifies the provider's signature/handshake so inbound webhooks can't be spoofed.</summary>
    bool VerifyWebhookSignature(string rawPayload, string? signatureHeader);
}
