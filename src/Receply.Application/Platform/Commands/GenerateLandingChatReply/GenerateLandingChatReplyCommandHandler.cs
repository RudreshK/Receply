using System.Text.Json;
using MediatR;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;
using Receply.Application.Common;
using Receply.Domain.Platform;
using Receply.Domain.Tenancy;

namespace Receply.Application.Platform.Commands.GenerateLandingChatReply;

/// <summary>Sales-assistant chat for anonymous visitors on the marketing site (see the landing-page
/// chat bubble). Deliberately separate from the customer-facing AI receptionist flow
/// (GenerateAiReplyCommandHandler): no tenant, no conversation persisted in the DB - the browser
/// just replays the message history each turn - and its one job is answering questions about
/// Receply itself and capturing a BusinessEnquiry.</summary>
public class GenerateLandingChatReplyCommandHandler(IAiConversationAgent agent, IApplicationDbContext db)
    : IRequestHandler<GenerateLandingChatReplyCommand, LandingChatReplyDto>
{
    private const int MaxToolCallRounds = 4;
    private const string SubmitEnquiryTool = "submit_business_enquiry";

    private const string SystemPrompt = """
        You are Receply's sales assistant, chatting with a visitor on the Receply marketing website. This
        visitor does not yet have a Receply account - you are not talking to an existing customer.

        What Receply is: a WhatsApp AI receptionist for small businesses (clinics, salons, gyms, car
        service centers, and similar). It auto-replies to customers on the business's own WhatsApp
        number 24/7 - answering questions about services/prices/hours, and booking, rescheduling, or
        cancelling appointments. Every plan starts with a free 14-day trial, no credit card required.

        Your job:
        - Answer questions about what Receply does, how it works, and pricing in general terms.
        - For anyone interested in signing up, naturally collect: business name, business type (clinic,
          salon, gym, car service, or other), their name, and a phone number. Email is optional.
        - Once you have at least business name, their name, and a phone number, call submit_business_enquiry
          with what you have. Don't call it more than once in the same conversation, and don't ask for
          information you already have.
        - After submitting, tell them the team will reach out soon.

        Guidelines:
        - Be warm, concise, and conversational - like a helpful live agent, not a form. Plain text only,
          no markdown formatting (no asterisks, no headers).
        - Keep replies short (1-3 sentences unless explaining a feature in more detail was asked for).
        - Only discuss Receply itself and collecting their enquiry. If asked about anything else -
          general knowledge, other products, coding help, anything unrelated - politely redirect to how
          you can help them with Receply.
        """;

    private static readonly AiTool SubmitEnquiryToolDefinition = new(
        SubmitEnquiryTool,
        "Submit this visitor's business enquiry so the Receply team can follow up. Call once you have at least business_name, contact_name, and phone_number.",
        """
        {
          "type": "object",
          "properties": {
            "business_name": { "type": "string" },
            "business_type": { "type": "string", "description": "One of: Clinic, Salon, Gym, CarService, Other." },
            "contact_name": { "type": "string" },
            "phone_number": { "type": "string" },
            "email": { "type": "string", "description": "Optional." }
          },
          "required": ["business_name", "contact_name", "phone_number"]
        }
        """);

    public async Task<LandingChatReplyDto> Handle(GenerateLandingChatReplyCommand request, CancellationToken cancellationToken)
    {
        var messages = request.History
            .Select(m => AiMessage.FromText(m.Role == "assistant" ? AiMessageRole.Assistant : AiMessageRole.User, m.Text))
            .Append(AiMessage.FromText(AiMessageRole.User, request.Message))
            .ToList();

        string? finalText = null;
        var submitted = false;

        for (var round = 0; round < MaxToolCallRounds; round++)
        {
            var turn = await agent.GetNextTurnAsync(
                new AiConversationRequest(SystemPrompt, messages, [SubmitEnquiryToolDefinition]), cancellationToken);
            var toolUses = turn.GetToolUses();

            if (toolUses.Count == 0)
            {
                finalText = WhatsAppTextFormatter.Normalize(turn.GetText() ?? "");
                break;
            }

            messages.Add(new AiMessage(AiMessageRole.Assistant, turn.Content));

            var toolResults = new List<AiContentBlock>();
            foreach (var toolUse in toolUses)
            {
                var (resultJson, isError) = await SubmitEnquiryAsync(toolUse.ArgumentsJson, cancellationToken);
                if (!isError) submitted = true;
                toolResults.Add(new AiToolResultBlock(toolUse.Id, resultJson, isError));
            }

            messages.Add(new AiMessage(AiMessageRole.User, toolResults));
        }

        if (string.IsNullOrWhiteSpace(finalText))
            finalText = "Thanks for reaching out! I've passed this along to our team and they'll get back to you shortly.";

        return new LandingChatReplyDto(finalText, submitted);
    }

    private async Task<(string ResultJson, bool IsError)> SubmitEnquiryAsync(string argumentsJson, CancellationToken cancellationToken)
    {
        try
        {
            using var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = args.RootElement;

            var businessName = GetString(root, "business_name");
            var contactName = GetString(root, "contact_name");
            var phoneNumber = GetString(root, "phone_number");
            var email = TryGetString(root, "email", out var em) ? em : null;
            var businessTypeRaw = TryGetString(root, "business_type", out var bt) ? bt : null;

            if (!Enum.TryParse<BusinessType>(businessTypeRaw, ignoreCase: true, out var businessType))
                businessType = BusinessType.Other;

            var enquiry = BusinessEnquiry.Create(businessName, businessType, contactName, phoneNumber, email, "Submitted via landing page chat.");
            db.BusinessEnquiries.Add(enquiry);
            await db.SaveChangesAsync(cancellationToken);

            return ("""{"status":"submitted"}""", false);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return ($$"""{"error":"{{ex.Message}}"}""", true);
        }
    }

    private static string GetString(JsonElement element, string propertyName) =>
        TryGetString(element, propertyName, out var value)
            ? value
            : throw new InvalidOperationException($"Missing required argument '{propertyName}'.");

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString()!;
            return true;
        }

        value = "";
        return false;
    }
}
