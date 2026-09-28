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

/// <summary>Runs on a schedule (registered in Program.cs) and sends a WhatsApp reminder for each
/// appointment starting within the reminder window, once each.</summary>
public class AppointmentReminderJob(
    IApplicationDbContext db,
    IChannelProvider channelProvider,
    IConversationNotifier conversationNotifier,
    IFeatureAccess featureAccess,
    ILogger<AppointmentReminderJob> logger) : IJob
{
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var windowEnd = now + ReminderWindow;

        var dueAppointments = await db.Appointments
            .Where(a => a.Status == AppointmentStatus.Booked && a.ReminderSentUtc == null
                        && a.StartUtc >= now && a.StartUtc <= windowEnd)
            .ToListAsync(context.CancellationToken);

        logger.LogInformation("Found {Count} appointment(s) due for a reminder.", dueAppointments.Count);

        foreach (var appointment in dueAppointments)
        {
            try
            {
                await SendReminderAsync(appointment, now, context.CancellationToken);
            }
            catch (Exception ex)
            {
                // One appointment's reminder failing (a transient WhatsApp API error, a missing
                // channel account, ...) must never stop the rest of this run's reminders from going out.
                logger.LogError(ex, "Failed to send a reminder for appointment {AppointmentId}.", appointment.Id);
            }
        }
    }

    private async Task SendReminderAsync(Appointment appointment, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!await featureAccess.IsEnabledAsync(appointment.TenantId, Feature.AppointmentReminders, cancellationToken))
            return;

        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == appointment.ClientId, cancellationToken);
        var service = await db.Services.FirstOrDefaultAsync(s => s.Id == appointment.ServiceId, cancellationToken);
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == appointment.TenantId, cancellationToken);
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.TenantId == appointment.TenantId && c.Type == ChannelType.WhatsApp, cancellationToken);

        if (client is null || service is null || tenant is null || channelAccount is null)
        {
            logger.LogWarning(
                "Skipping reminder for appointment {AppointmentId} - missing client, service, tenant, or WhatsApp channel account.",
                appointment.Id);
            return;
        }

        var conversation = await db.Conversations
            .Where(c => c.TenantId == appointment.TenantId && c.ClientId == client.Id)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            // Every client today originates from an inbound WhatsApp message, so this shouldn't
            // happen - but a reminder has nowhere to attach to (and no Message.ConversationId to
            // satisfy) without one, so skip rather than fail the whole run.
            logger.LogWarning("Skipping reminder for appointment {AppointmentId} - client has no conversation yet.", appointment.Id);
            return;
        }

        var localStart = ConvertToLocal(appointment.StartUtc, tenant.TimeZoneId);
        var text = $"Hi! This is a reminder for your {service.Name} appointment with {tenant.Name} " +
                   $"on {localStart:dddd, MMMM d 'at' h:mm tt}. Reply here if you need to reschedule or cancel.";

        var message = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, text);
        appointment.MarkReminderSent(now);
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            conversation.TenantId,
            conversation.Id,
            new ConversationMessageDto(message.Id, message.Direction, message.Sender, message.Body, message.SentAtUtc, message.DeliveryStatus),
            cancellationToken);

        await ConversationMessagingHelper.SendAndTrackAsync(
            db, channelProvider, conversationNotifier, conversation, message, channelAccount, client.PhoneNumber, text, cancellationToken);
    }

    private static DateTimeOffset ConvertToLocal(DateTimeOffset utc, string timeZoneId)
    {
        try
        {
            return TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        }
        catch (TimeZoneNotFoundException)
        {
            return utc;
        }
    }
}
