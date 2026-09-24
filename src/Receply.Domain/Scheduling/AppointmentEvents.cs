using MediatR;

namespace Receply.Domain.Scheduling;

public record AppointmentBooked(Guid TenantId, Guid AppointmentId) : INotification;

public record AppointmentRescheduled(Guid TenantId, Guid AppointmentId, DateTimeOffset PreviousStartUtc, DateTimeOffset NewStartUtc) : INotification;

public record AppointmentCancelled(Guid TenantId, Guid AppointmentId, string? Reason) : INotification;

public record AppointmentCompleted(Guid TenantId, Guid AppointmentId) : INotification;
