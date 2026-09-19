using Microsoft.EntityFrameworkCore;
using Quartz;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Workers.Jobs;

/// <summary>
/// Runs on a schedule (registered in Program.cs) and sends WhatsApp reminders for appointments
/// starting soon. This is a skeleton: it finds due appointments but leaves the actual "send via
/// the customer's channel + mark as reminded" steps as the next piece to build.
/// </summary>
public class AppointmentReminderJob(IApplicationDbContext db, ILogger<AppointmentReminderJob> logger) : IJob
{
    private static readonly TimeSpan ReminderWindow = TimeSpan.FromHours(24);

    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTimeOffset.UtcNow;
        var windowEnd = now + ReminderWindow;

        var dueAppointments = await db.Appointments
            .Where(a => a.Status == AppointmentStatus.Booked && a.StartUtc >= now && a.StartUtc <= windowEnd)
            .ToListAsync(context.CancellationToken);

        logger.LogInformation("Found {Count} appointment(s) due for a reminder.", dueAppointments.Count);

        // TODO: resolve each appointment's customer + channel account, send a reminder via
        // IChannelProvider, and track that a reminder was already sent to avoid duplicates.
    }
}
