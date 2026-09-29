using Receply.Domain.Common;

namespace Receply.Domain.Scheduling;

/// <summary>
/// A staff-authored care instruction (medication reminder, diet note, exercise reminder, ...) queued
/// to go out to a client by WhatsApp a few days after a completed appointment. The instructions are
/// entered by the staff member/doctor themselves rather than invented by the AI, since this is exactly
/// the kind of specific, personal guidance that shouldn't be hallucinated.
/// </summary>
public class AppointmentFollowUp : TenantOwnedEntity
{
    public Guid AppointmentId { get; private set; }
    public string Instructions { get; private set; } = default!;
    public string SentByName { get; private set; } = default!;
    public DateTimeOffset ScheduledForUtc { get; private set; }
    public DateTimeOffset? SentUtc { get; private set; }

    private AppointmentFollowUp() { }

    public static AppointmentFollowUp Schedule(
        Guid tenantId, Guid appointmentId, string instructions, string sentByName, DateTimeOffset scheduledForUtc)
    {
        if (string.IsNullOrWhiteSpace(instructions))
            throw new ArgumentException("Follow-up instructions are required.", nameof(instructions));

        var followUp = new AppointmentFollowUp
        {
            AppointmentId = appointmentId,
            Instructions = instructions.Trim(),
            SentByName = sentByName,
            ScheduledForUtc = scheduledForUtc
        };
        followUp.TenantId = tenantId;
        return followUp;
    }

    public void MarkSent(DateTimeOffset sentAtUtc) => SentUtc = sentAtUtc;
}
