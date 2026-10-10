using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Domain.Channels;

namespace Receply.Application.Tenancy.Commands.GenerateStaffAiReply;

public class GenerateStaffAiReplyCommandHandler(
    IApplicationDbContext db,
    IAiConversationAgent agent,
    IAiToolExecutor toolExecutor,
    IChannelProvider channelProvider)
    : IRequestHandler<GenerateStaffAiReplyCommand>
{
    private const int MaxToolCallRounds = 6;

    public async Task Handle(GenerateStaffAiReplyCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken);
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.TenantId == request.TenantId && c.Type == ChannelType.WhatsApp && c.IsActive, cancellationToken);

        if (staff is null || channelAccount is null)
            return;

        var systemPrompt = $"""
            You are Receply's internal assistant for {staff.FullName} ({staff.Role}), a staff member managing this business over WhatsApp.

            Strict scope - the only things you may do:
            - Update {staff.FullName}'s own name, email, or phone number, using update_my_details.
            - Add, remove, or list business holidays, using add_holiday, remove_holiday, and list_holidays.

            Guidelines:
            - Always use the tools to make changes or look things up - never claim to have done something without calling the matching tool.
            - Be concise and confirm what changed once a tool succeeds.
            - This is WhatsApp, not standard Markdown: for bold text use a single asterisk on each side (*like this*), never double asterisks (**like this**).
            - If the request is about anything else - customers, appointments, other staff, or anything unrelated to holidays and this staff member's own details - do not attempt it. Reply with exactly: "Sorry, I can only help with holidays and your own details here - please use the dashboard for anything else."
            """;

        var messages = new List<AiMessage> { AiMessage.FromText(AiMessageRole.User, request.MessageBody) };
        var toolContext = new AiToolContext(request.TenantId, Guid.Empty, Guid.Empty, staff.Id);

        string? finalText = null;

        for (var round = 0; round < MaxToolCallRounds; round++)
        {
            var turn = await agent.GetNextTurnAsync(new AiConversationRequest(systemPrompt, messages, StaffAiToolCatalog.All), cancellationToken);
            var toolUses = turn.GetToolUses();

            if (toolUses.Count == 0)
            {
                finalText = WhatsAppTextFormatter.Normalize(turn.GetText());
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

        finalText ??= "Sorry, I couldn't complete that - please try again or use the dashboard.";

        await channelProvider.SendMessageAsync(channelAccount, staff.PhoneNumber, finalText, cancellationToken);
    }
}
