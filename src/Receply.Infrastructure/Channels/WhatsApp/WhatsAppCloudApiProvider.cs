using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Receply.Application.Channels;
using Receply.Domain.Channels;

namespace Receply.Infrastructure.Channels.WhatsApp;

/// <summary>IChannelProvider implementation against Meta's WhatsApp Cloud API (Graph API).</summary>
public class WhatsAppCloudApiProvider(
    HttpClient httpClient,
    IOptions<WhatsAppOptions> options,
    ILogger<WhatsAppCloudApiProvider> logger) : IChannelProvider
{
    private readonly WhatsAppOptions _options = options.Value;

    public ChannelType Type => ChannelType.WhatsApp;

    public async Task<string> SendMessageAsync(ChannelAccount account, string toPhoneNumber, string body, CancellationToken cancellationToken = default)
    {
        var url = $"{_options.GraphApiBaseUrl}/{_options.ApiVersion}/{account.ExternalId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneNumber,
            type = "text",
            text = new { body }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("WhatsApp send failed ({Status}): {Error}", response.StatusCode, responseBody);
            response.EnsureSuccessStatusCode();
        }

        var result = JsonSerializer.Deserialize<SendMessageResponse>(responseBody, JsonOptions);
        return result?.Messages?.FirstOrDefault()?.Id
            ?? throw new InvalidOperationException($"WhatsApp send succeeded but returned no message id: {responseBody}");
    }

    public async Task ShowTypingIndicatorAsync(ChannelAccount account, string inboundExternalMessageId, CancellationToken cancellationToken = default)
    {
        // A single call that both marks the customer's message as read (their blue ticks) and shows
        // a "typing…" indicator on their side for up to ~25s or until we actually send a reply -
        // keeps them from wandering off while the AI/tool-call loop or a staff member is composing.
        var url = $"{_options.GraphApiBaseUrl}/{_options.ApiVersion}/{account.ExternalId}/messages";

        var payload = new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = inboundExternalMessageId,
            typing_indicator = new { type = "text" }
        };

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(payload) };
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.AccessToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning("WhatsApp mark-as-read/typing indicator failed ({Status}): {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Best-effort UX touch - never let this hold up or fail message ingestion.
            logger.LogWarning(ex, "Failed to show typing indicator for message {MessageId}.", inboundExternalMessageId);
        }
    }

    public IReadOnlyCollection<InboundChannelMessage> ParseInboundWebhook(string rawPayload)
    {
        var envelope = JsonSerializer.Deserialize<WebhookEnvelope>(rawPayload, JsonOptions);
        var results = new List<InboundChannelMessage>();

        foreach (var entry in envelope?.Entry ?? [])
        foreach (var change in entry.Changes ?? [])
        {
            var phoneNumberId = change.Value?.Metadata?.PhoneNumberId;

            var namesByWaId = (change.Value?.Contacts ?? [])
                .Where(c => c.WaId is not null && c.Profile?.Name is not null)
                .ToDictionary(c => c.WaId!, c => c.Profile!.Name!);

            foreach (var message in change.Value?.Messages ?? [])
            {
                if (phoneNumberId is null || message.From is null || message.Text?.Body is null)
                    continue;

                var receivedAt = long.TryParse(message.Timestamp, out var unixSeconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
                    : DateTimeOffset.UtcNow;

                namesByWaId.TryGetValue(message.From, out var displayName);

                results.Add(new InboundChannelMessage(phoneNumberId, message.From, message.Text.Body, receivedAt, displayName, message.Id));
            }
        }

        return results;
    }

    public IReadOnlyCollection<InboundStatusUpdate> ParseStatusUpdates(string rawPayload)
    {
        var envelope = JsonSerializer.Deserialize<WebhookEnvelope>(rawPayload, JsonOptions);
        var results = new List<InboundStatusUpdate>();

        foreach (var entry in envelope?.Entry ?? [])
        foreach (var change in entry.Changes ?? [])
        {
            var phoneNumberId = change.Value?.Metadata?.PhoneNumberId;
            if (phoneNumberId is null)
                continue;

            foreach (var status in change.Value?.Statuses ?? [])
            {
                if (status.Id is null || status.Status is null)
                    continue;

                var updatedAt = long.TryParse(status.Timestamp, out var unixSeconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
                    : DateTimeOffset.UtcNow;

                results.Add(new InboundStatusUpdate(phoneNumberId, status.Id, status.Status, updatedAt));
            }
        }

        return results;
    }

    public bool VerifyWebhookSignature(string rawPayload, string? signatureHeader)
    {
        if (string.IsNullOrEmpty(signatureHeader) || !signatureHeader.StartsWith("sha256=", StringComparison.Ordinal))
            return false;

        var expectedHex = signatureHeader["sha256=".Length..];
        var keyBytes = Encoding.UTF8.GetBytes(_options.AppSecret);
        var payloadBytes = Encoding.UTF8.GetBytes(rawPayload);

        var computedHash = HMACSHA256.HashData(keyBytes, payloadBytes);
        var computedHex = Convert.ToHexString(computedHash);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(computedHex),
            Convert.FromHexString(expectedHex.ToUpperInvariant()));
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class WebhookEnvelope
    {
        [JsonPropertyName("entry")] public List<WebhookEntry>? Entry { get; set; }
    }

    private sealed class WebhookEntry
    {
        [JsonPropertyName("changes")] public List<WebhookChange>? Changes { get; set; }
    }

    private sealed class WebhookChange
    {
        [JsonPropertyName("value")] public WebhookValue? Value { get; set; }
    }

    private sealed class WebhookValue
    {
        [JsonPropertyName("metadata")] public WebhookMetadata? Metadata { get; set; }
        [JsonPropertyName("contacts")] public List<WebhookContact>? Contacts { get; set; }
        [JsonPropertyName("messages")] public List<WebhookMessage>? Messages { get; set; }
        [JsonPropertyName("statuses")] public List<WebhookStatus>? Statuses { get; set; }
    }

    private sealed class WebhookMetadata
    {
        [JsonPropertyName("phone_number_id")] public string? PhoneNumberId { get; set; }
    }

    private sealed class WebhookContact
    {
        [JsonPropertyName("wa_id")] public string? WaId { get; set; }
        [JsonPropertyName("profile")] public WebhookProfile? Profile { get; set; }
    }

    private sealed class WebhookProfile
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
    }

    private sealed class WebhookMessage
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("from")] public string? From { get; set; }
        [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
        [JsonPropertyName("text")] public WebhookText? Text { get; set; }
    }

    private sealed class WebhookText
    {
        [JsonPropertyName("body")] public string? Body { get; set; }
    }

    private sealed class WebhookStatus
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
    }

    private sealed class SendMessageResponse
    {
        [JsonPropertyName("messages")] public List<SendMessageId>? Messages { get; set; }
    }

    private sealed class SendMessageId
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
    }
}
