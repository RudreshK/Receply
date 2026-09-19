using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Commands.GenerateAiReply;

public class GenerateAiReplyCommandHandler(
    IApplicationDbContext db,
    IAiConversationAgent agent,
    IAiToolExecutor toolExecutor,
    IChannelProvider channelProvider)
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

        var systemPrompt = await BuildSystemPromptAsync(tenant.Id, tenant.Name, tenant.BusinessType.ToString(), tenant.TimeZoneId, cancellationToken);

        var messages = conversation.Messages
            .Where(m => m.Sender is MessageSender.Client or MessageSender.Ai)
            .OrderBy(m => m.SentAtUtc)
            .Select(m => AiMessage.FromText(m.Sender == MessageSender.Client ? AiMessageRole.User : AiMessageRole.Assistant, m.Body))
            .ToList();

        var toolContext = new AiToolContext(tenant.Id, conversation.Id, client.Id);

        string? finalText = null;

        for (var round = 0; round < MaxToolCallRounds; round++)
        {
            var turn = await agent.GetNextTurnAsync(new AiConversationRequest(systemPrompt, messages, AiToolCatalog.All), cancellationToken);
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

        if (string.IsNullOrWhiteSpace(finalText))
        {
            finalText = "Sorry, I'm having trouble with that right now - let me get a team member to help you.";
            conversation.RequestHandoff("AI could not resolve the request after several attempts.");
        }

        conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, finalText);
        await db.SaveChangesAsync(cancellationToken);

        await channelProvider.SendMessageAsync(channelAccount, client.PhoneNumber, finalText, cancellationToken);
    }

    private async Task<string> BuildSystemPromptAsync(Guid tenantId, string tenantName, string businessType, string timeZoneId, CancellationToken cancellationToken)
    {
        var services = await db.Services
            .Where(s => s.TenantId == tenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var nowLocal = TryGetNowLocal(timeZoneId);

        var sb = new StringBuilder();
        sb.AppendLine($"You are the AI receptionist for {tenantName}, a {businessType.ToLowerInvariant()} business.");
        sb.AppendLine($"The current date and time where the business is located is {nowLocal:dddd, MMMM d, yyyy 'at' h:mm tt} ({timeZoneId}).");
        sb.AppendLine();
        sb.AppendLine("Your job: answer customer questions, and help them book, reschedule, or cancel appointments over this WhatsApp chat.");
        sb.AppendLine();
        sb.AppendLine("Guidelines:");
        sb.AppendLine("- Be warm, concise, and conversational. Keep replies short (2-4 sentences unless listing options).");
        sb.AppendLine("- Always use the provided tools to check real availability and to actually perform bookings, reschedules, or cancellations. Never claim to have done something without calling the matching tool first.");
        sb.AppendLine("- Never invent services, prices, or appointment times that weren't returned by a tool.");
        sb.AppendLine("- If you're not confident you understood the request, ask a short clarifying question rather than guessing.");
        sb.AppendLine("- If the customer explicitly asks for a human, or you can't resolve their request after a couple of tries, call request_human_handoff.");
        sb.AppendLine();

        if (services.Count > 0)
        {
            sb.AppendLine("Services offered:");
            foreach (var service in services)
                sb.AppendLine($"- {service.Name}: {service.Duration.TotalMinutes} min, {service.Price:C}");
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
