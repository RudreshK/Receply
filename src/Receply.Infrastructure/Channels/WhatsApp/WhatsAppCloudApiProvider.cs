using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

    public async Task SendMessageAsync(ChannelAccount account, string toPhoneNumber, string body, CancellationToken cancellationToken = default)
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

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("WhatsApp send failed ({Status}): {Error}", response.StatusCode, error);
            response.EnsureSuccessStatusCode();
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
            foreach (var message in change.Value?.Messages ?? [])
            {
                if (phoneNumberId is null || message.From is null || message.Text?.Body is null)
                    continue;

                var receivedAt = long.TryParse(message.Timestamp, out var unixSeconds)
                    ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
                    : DateTimeOffset.UtcNow;

                results.Add(new InboundChannelMessage(phoneNumberId, message.From, message.Text.Body, receivedAt));
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
        [JsonPropertyName("messages")] public List<WebhookMessage>? Messages { get; set; }
    }

    private sealed class WebhookMetadata
    {
        [JsonPropertyName("phone_number_id")] public string? PhoneNumberId { get; set; }
    }

    private sealed class WebhookMessage
    {
        [JsonPropertyName("from")] public string? From { get; set; }
        [JsonPropertyName("timestamp")] public string? Timestamp { get; set; }
        [JsonPropertyName("text")] public WebhookText? Text { get; set; }
    }

    private sealed class WebhookText
    {
        [JsonPropertyName("body")] public string? Body { get; set; }
    }
}
