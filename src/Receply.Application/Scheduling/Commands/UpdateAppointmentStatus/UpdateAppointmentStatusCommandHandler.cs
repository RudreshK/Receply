using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.AiEngine;
using Receply.Application.AiEngine.Contracts;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Features;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Application.Scheduling.Commands.UpdateAppointmentStatus;

public class UpdateAppointmentStatusCommandHandler(
    IApplicationDbContext db,
    IAiConversationAgent agent,
    IChannelProvider channelProvider,
    IConversationNotifier conversationNotifier,
    IFeatureAccess featureAccess)
    : IRequestHandler<UpdateAppointmentStatusCommand>
{
    public async Task Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(
            a => a.Id == request.AppointmentId && a.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.AppointmentId}' not found.");

        // Idempotent: re-applying the same outcome (a double click, a retried request) is a no-op
        // rather than re-sending the client a second copy of the outcome message.
        if (appointment.Status == request.NewStatus)
            return;

        if (request.NewStatus == AppointmentStatus.Completed)
            appointment.Complete();
        else
            appointment.MarkNoShow();

        await db.SaveChangesAsync(cancellationToken);

        if (await featureAccess.IsEnabledAsync(request.TenantId, Feature.AppointmentOutcomeMessages, cancellationToken))
            await SendOutcomeMessageAsync(appointment, cancellationToken);
    }

    private async Task SendOutcomeMessageAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == appointment.ClientId, cancellationToken);
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId, cancellationToken);
        var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == appointment.ResourceId, cancellationToken);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == appointment.TenantId, cancellationToken);
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.TenantId == appointment.TenantId && c.Type == ChannelType.WhatsApp, cancellationToken);

        if (client is null || service is null || resource is null || tenant is null || channelAccount is null)
            return;

        if (!tenant.HasActiveAccess(DateTimeOffset.UtcNow))
            return;

        var conversation = await db.Conversations
            .Where(c => c.TenantId == appointment.TenantId && c.ClientId == client.Id)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
            return;

        var text = await DraftMessageAsync(appointment, tenant, service.Name, resource.Name, client.FullName, cancellationToken);

        var message = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, text);
        appointment.MarkOutcomeNotificationSent(DateTimeOffset.UtcNow);
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            conversation.TenantId,
            conversation.Id,
            new ConversationMessageDto(message.Id, message.Direction, message.Sender, message.Body, message.SentAtUtc, message.DeliveryStatus),
            cancellationToken);

        await ConversationMessagingHelper.SendAndTrackAsync(
            db, channelProvider, conversationNotifier, conversation, message, channelAccount, client.PhoneNumber, text, cancellationToken);
    }

    private async Task<string> DraftMessageAsync(
        Appointment appointment, Tenant tenant, string serviceName, string resourceName, string? clientName, CancellationToken cancellationToken)
    {
        var greetingName = string.IsNullOrWhiteSpace(clientName) ? "there" : clientName.Split(' ')[0];

        var systemPrompt = appointment.Status == AppointmentStatus.Completed
            ? $"""
               You are writing one short WhatsApp message on behalf of {tenant.Name}, a {tenant.BusinessType.ToString().ToLowerInvariant()} business,
               to a client named {greetingName} right after their {serviceName} appointment with {resourceName} just finished.

               The message should:
               - Thank them warmly for coming in.
               - Wish them well / a speedy recovery or good results, in a tone that fits a {tenant.BusinessType.ToString().ToLowerInvariant()} business.
               - Invite them to message anytime if they need anything else.
               - Sign off with the business name.
               Keep it to 2-4 sentences, warm and genuine - not corporate or generic. Output ONLY the message text, nothing else.
               """
            : $"""
               You are writing one short WhatsApp message on behalf of {tenant.Name}, a {tenant.BusinessType.ToString().ToLowerInvariant()} business,
               to a client named {greetingName} who missed their {serviceName} appointment with {resourceName} today.

               The message should:
               - Gently let them know they were missed / {resourceName} was expecting them.
               - Reassure them it's no problem and offer to reschedule.
               - Ask them to reply with a day/time that works.
               Keep it to 2-4 sentences, warm and non-judgmental - it should read like a caring human wrote it, not an automated notice.
               Output ONLY the message text, nothing else.
               """;

        var turn = await agent.GetNextTurnAsync(
            new AiConversationRequest(systemPrompt, [AiMessage.FromText(AiMessageRole.User, "Write the message now.")], []),
            cancellationToken);

        var text = turn.GetText();
        if (!string.IsNullOrWhiteSpace(text))
            return text.Trim();

        // Fallback if the model returns nothing usable - still send something rather than silently
        // skipping the client's notification.
        return appointment.Status == AppointmentStatus.Completed
            ? $"Thanks for coming in today, {greetingName}! Hope you're doing well - reach out anytime if you need anything. - {tenant.Name}"
            : $"Hi {greetingName}, just checking in - we missed you for your {serviceName} appointment with {resourceName} today. No worries, let us know when you'd like to reschedule.";
    }
}
