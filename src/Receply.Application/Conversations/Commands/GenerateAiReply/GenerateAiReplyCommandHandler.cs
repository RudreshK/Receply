using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Features;
using Receply.Domain.Conversations;
using Receply.Domain.Tenancy;

namespace Receply.Application.Conversations.Commands.GenerateAiReply;

public class GenerateAiReplyCommandHandler(
    IApplicationDbContext db,
    IAiConversationAgent agent,
    IAiToolExecutor toolExecutor,
    IChannelProvider channelProvider,
    IConversationNotifier conversationNotifier,
    IFeatureAccess featureAccess)
    : IRequestHandler<GenerateAiReplyCommand>
{
    private const int MaxToolCallRounds = 6;

    public async Task Handle(GenerateAiReplyCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.TenantId == request.TenantId, cancellationToken);

        // A human has already taken over (or the thread is closed) - the AI stays quiet.
        if (conversation is null || conversation.Status is HandoffStatus.HandoffRequested or HandoffStatus.HumanAssigned or HandoffStatus.Resolved)
            return;

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == conversation.ClientId, cancellationToken);
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(c => c.Id == conversation.ChannelAccountId, cancellationToken);

        if (tenant is null || client is null || channelAccount is null)
            return;

        // No active trial/subscription - the AI stays completely silent. Appointment booking only
        // happens through this same AI tool-calling loop, so this one check is also what stops a
        // lapsed tenant from booking anything.
        if (!tenant.HasActiveAccess(DateTimeOffset.UtcNow))
            return;

        var canBook = await featureAccess.IsEnabledAsync(tenant.Id, Feature.AppointmentBooking, cancellationToken);
        var tools = canBook
            ? AiToolCatalog.All
            : AiToolCatalog.All.Where(t => t.Name is AiToolCatalog.ListServices or AiToolCatalog.RequestHumanHandoff).ToList();

        var systemPrompt = await BuildSystemPromptAsync(tenant.Id, tenant.Name, tenant.BusinessType.ToString(), tenant.TimeZoneId, canBook, cancellationToken);

        var messages = conversation.Messages
            .Where(m => m.Sender is MessageSender.Client or MessageSender.Ai)
            .OrderBy(m => m.SentAtUtc)
            .Select(m => AiMessage.FromText(m.Sender == MessageSender.Client ? AiMessageRole.User : AiMessageRole.Assistant, m.Body))
            .ToList();

        var toolContext = new AiToolContext(tenant.Id, conversation.Id, client.Id);

        string? finalText = null;

        for (var round = 0; round < MaxToolCallRounds; round++)
        {
            var turn = await agent.GetNextTurnAsync(new AiConversationRequest(systemPrompt, messages, tools), cancellationToken);
            var toolUses = turn.GetToolUses();

            if (toolUses.Count == 0)
            {
                finalText = turn.GetText();
                break;
            }

            messages.Add(new AiMessage(AiMessageRole.Assistant, turn.Content));

            var toolResults = new List<AiContentBlock>();
            foreach (var toolUse in toolUses)
            {
                var (resultJson, isError) = await toolExecutor.ExecuteAsync(toolUse.Name, toolUse.ArgumentsJson, toolContext, cancellationToken);
                toolResults.Add(new AiToolResultBlock(toolUse.Id, resultJson, isError));
            }

            messages.Add(new AiMessage(AiMessageRole.User, toolResults));
        }

        var escalated = string.IsNullOrWhiteSpace(finalText);
        if (escalated)
        {
            finalText = "Sorry, I'm having trouble with that right now - let me get a team member to help you.";
            conversation.RequestHandoff("AI could not resolve the request after several attempts.");
        }

        var reply = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, finalText);
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            conversation.TenantId,
            conversation.Id,
            new ConversationMessageDto(reply.Id, reply.Direction, reply.Sender, reply.Body, reply.SentAtUtc, reply.DeliveryStatus),
            cancellationToken);

        if (escalated)
            await conversationNotifier.StatusChangedAsync(conversation.TenantId, conversation.Id, conversation.Status, cancellationToken);

        await ConversationMessagingHelper.SendAndTrackAsync(
            db, channelProvider, conversationNotifier, conversation, reply, channelAccount, client.PhoneNumber, finalText, cancellationToken);
    }

    private async Task<string> BuildSystemPromptAsync(
        Guid tenantId, string tenantName, string businessType, string timeZoneId, bool canBook, CancellationToken cancellationToken)
    {
        var services = await db.Services
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var faqs = await db.Faqs
            .Where(f => f.TenantId == tenantId && f.IsActive)
            .OrderBy(f => f.Question)
            .ToListAsync(cancellationToken);

        var nowLocal = TryGetNowLocal(timeZoneId);

        var sb = new StringBuilder();
        sb.AppendLine($"You are the AI receptionist for {tenantName}, a {businessType.ToLowerInvariant()} business.");
        sb.AppendLine($"The current date and time where the business is located is {nowLocal:dddd, MMMM d, yyyy 'at' h:mm tt} ({timeZoneId}).");
        sb.AppendLine();
        sb.AppendLine(canBook
            ? "Your job: answer customer questions, help them book, reschedule, or cancel appointments, and answer questions about an appointment they already have (including whether it's confirmed and reminders for it) over this WhatsApp chat."
            : "Your job: answer customer questions about this business - services, prices, hours, and FAQs - over this WhatsApp chat. You cannot book, reschedule, or cancel appointments; if a customer wants to do that, tell them a team member will help and call request_human_handoff.");
        sb.AppendLine();
        sb.AppendLine("Guidelines:");
        sb.AppendLine("- Be warm, concise, and conversational. Keep replies short (2-4 sentences unless listing options).");
        sb.AppendLine(canBook
            ? "- Always use the provided tools to check real availability and to actually perform bookings, reschedules, or cancellations. Never claim to have done something without calling the matching tool first."
            : "- You have no booking tools. Never claim to have booked, rescheduled, or cancelled anything - hand those requests to a human instead.");
        sb.AppendLine("- Never invent services, prices, or appointment times that weren't returned by a tool.");
        if (canBook)
            sb.AppendLine("- If the customer asks about an existing appointment for any reason - confirming it, a reminder that didn't arrive, anything about a booking they already made - call list_my_appointments first so you're referencing their real appointment, not guessing.");
        sb.AppendLine("- If you're not confident you understood the request, ask a short clarifying question rather than guessing.");
        sb.AppendLine("- If the customer explicitly asks for a human, or you can't resolve their request after a couple of tries - including something you have no tool for, like a reminder that didn't send - call request_human_handoff instead of refusing. Reassure them about what you did confirm (e.g. the appointment itself) before handing off.");
        sb.AppendLine();
        sb.AppendLine("Strict scope - this is the most important rule:");
        sb.AppendLine(canBook
            ? "- You may ONLY discuss: this business's services/prices/hours/FAQs listed below; booking, rescheduling, or cancelling appointments; and questions about an appointment the customer already has, including reminders for it."
            : "- You may ONLY discuss: this business's services/prices/hours/FAQs listed below. You cannot look up or discuss a specific appointment - hand that off to a human.");
        sb.AppendLine("- If the customer asks anything outside that scope - general knowledge, other companies/products, coding help, personal advice, news, or anything unrelated to this business - do NOT answer it, even if you know the answer. Never draw on your own general knowledge for an off-topic request.");
        sb.AppendLine("- For an off-topic request, reply with exactly this and nothing else: \"Sorry, I can't help with that here. Is there anything about booking or our services I can help you with?\"");
        sb.AppendLine();

        if (services.Count > 0)
        {
            sb.AppendLine("Services offered:");
            foreach (var service in services)
                sb.AppendLine($"- {service.Name}: {service.Duration.TotalMinutes} min, {service.Price:C}");
        }

        if (faqs.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Frequently asked questions - use these verbatim when a customer asks something covered here:");
            foreach (var faq in faqs)
                sb.AppendLine($"- Q: {faq.Question}\n  A: {faq.Answer}");
        }

        return sb.ToString();
    }

    private static DateTimeOffset TryGetNowLocal(string timeZoneId)
    {
        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateTimeOffset.UtcNow;
        }
    }
}
