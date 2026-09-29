using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

public enum AppointmentStatus
{
    Booked,
    Rescheduled,
    Cancelled,
    Completed,
    NoShow
}

public class Appointment : TenantOwnedEntity
{
    public Guid ClientId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid ResourceId { get; private set; }
    public Guid BranchId { get; private set; }
    public DateTimeOffset StartUtc { get; private set; }
    public DateTimeOffset EndUtc { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? ReminderSentUtc { get; private set; }
    public DateTimeOffset? OutcomeNotificationSentUtc { get; private set; }

    private Appointment() { }

    public static Appointment Book(
        Guid tenantId, Guid clientId, Guid serviceId, Guid resourceId, Guid branchId,
        DateTimeOffset startUtc, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive.", nameof(duration));

        var appointment = new Appointment
        {
            ClientId = clientId,
            ServiceId = serviceId,
            ResourceId = resourceId,
            BranchId = branchId,
            StartUtc = startUtc,
            EndUtc = startUtc + duration,
            Status = AppointmentStatus.Booked
        };
        appointment.TenantId = tenantId;
        appointment.Raise(new AppointmentBooked(tenantId, appointment.Id));
        return appointment;
    }

    public void Reschedule(DateTimeOffset newStartUtc, TimeSpan duration)
    {
        EnsureModifiable();

        var previousStart = StartUtc;
        StartUtc = newStartUtc;
        EndUtc = newStartUtc + duration;
        Status = AppointmentStatus.Rescheduled;
        ReminderSentUtc = null; // a reminder for the old time would be wrong - eligible to remind again for the new one

        Raise(new AppointmentRescheduled(TenantId, Id, previousStart, newStartUtc));
    }

    public void Cancel(string? reason = null)
    {
        EnsureModifiable();

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;

        Raise(new AppointmentCancelled(TenantId, Id, reason));
    }

    public void Complete()
    {
        if (Status is AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot complete a cancelled appointment.");

        Status = AppointmentStatus.Completed;
        OutcomeNotificationSentUtc = null; // a status flip from NoShow back to Completed is eligible for its own outcome message
        Raise(new AppointmentCompleted(TenantId, Id));
    }

    public void MarkNoShow()
    {
        if (Status is AppointmentStatus.Cancelled)
            throw new InvalidOperationException("Cannot mark a cancelled appointment as a no-show.");

        Status = AppointmentStatus.NoShow;
        OutcomeNotificationSentUtc = null;
    }

    public void MarkReminderSent(DateTimeOffset sentAtUtc) => ReminderSentUtc = sentAtUtc;

    public void MarkOutcomeNotificationSent(DateTimeOffset sentAtUtc) => OutcomeNotificationSentUtc = sentAtUtc;

    private void EnsureModifiable()
    {
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed)
            throw new InvalidOperationException($"Cannot modify an appointment in status '{Status}'.");
    }
}
