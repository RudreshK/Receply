using Microsoft.EntityFrameworkCore;
using Quartz;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Features;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Workers.Jobs;

/// <summary>Runs on a schedule and sends each staff-authored post-appointment follow-up (medication,
/// diet, exercise reminders, ...) once its scheduled day/time has arrived.</summary>
public class AppointmentFollowUpJob(
    IApplicationDbContext db,
    IChannelProvider channelProvider,
    IConversationNotifier conversationNotifier,
    IFeatureAccess featureAccess,
    ILogger<AppointmentFollowUpJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;

        var dueFollowUps = await db.AppointmentFollowUps
            .Where(f => f.SentUtc == null && f.ScheduledForUtc <= now)
            .ToListAsync(context.CancellationToken);

        logger.LogInformation("Found {Count} appointment follow-up(s) due to send.", dueFollowUps.Count);

        foreach (var followUp in dueFollowUps)
        {
            try
            {
                await SendFollowUpAsync(followUp, now, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send appointment follow-up {FollowUpId}.", followUp.Id);
            }
        }
    }

    private async Task SendFollowUpAsync(AppointmentFollowUp followUp, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!await featureAccess.IsEnabledAsync(followUp.TenantId, Feature.AppointmentFollowUps, cancellationToken))
            return;

        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.Id == followUp.AppointmentId, cancellationToken);
        if (appointment is null)
        {
            logger.LogWarning("Skipping follow-up {FollowUpId} - its appointment no longer exists.", followUp.Id);
            return;
        }

        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == appointment.ClientId, cancellationToken);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == followUp.TenantId, cancellationToken);
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.TenantId == followUp.TenantId && c.Type == ChannelType.WhatsApp, cancellationToken);

        if (client is null || tenant is null || channelAccount is null)
        {
            logger.LogWarning("Skipping follow-up {FollowUpId} - missing client, tenant, or WhatsApp channel account.", followUp.Id);
            return;
        }

        if (!tenant.HasActiveAccess(now))
            return;

        var conversation = await db.Conversations
            .Where(c => c.TenantId == followUp.TenantId && c.ClientId == client.Id)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            logger.LogWarning("Skipping follow-up {FollowUpId} - client has no conversation yet.", followUp.Id);
            return;
        }

        var greetingName = string.IsNullOrWhiteSpace(client.FullName) ? "there" : client.FullName.Split(' ')[0];
        var text = $"Hi {greetingName}, following up on your recent visit - {followUp.Instructions}\n\n- {followUp.SentByName}";

        var message = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, text);
        followUp.MarkSent(now);
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            conversation.TenantId,
            conversation.Id,
            new ConversationMessageDto(message.Id, message.Direction, message.Sender, message.Body, message.SentAtUtc, message.DeliveryStatus),
            cancellationToken);

        await ConversationMessagingHelper.SendAndTrackAsync(
            db, channelProvider, conversationNotifier, conversation, message, channelAccount, client.PhoneNumber, text, cancellationToken);
    }
}
