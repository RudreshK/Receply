using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>A service a staff member performs, and how long they take for one session of it.</summary>
public class StaffService : TenantOwnedEntity
{
    public Guid StaffId { get; private set; }
    public Guid ServiceId { get; private set; }
    public TimeSpan SessionDuration { get; private set; }

    private StaffService() { }

    public static StaffService Create(Guid tenantId, Guid staffId, Guid serviceId, TimeSpan sessionDuration)
    {
        if (sessionDuration <= TimeSpan.Zero)
            throw new ArgumentException("Session duration must be positive.", nameof(sessionDuration));

        var staffService = new StaffService
        {
            StaffId = staffId,
            ServiceId = serviceId,
            SessionDuration = sessionDuration
        };
        staffService.TenantId = tenantId;
        return staffService;
    }
}
