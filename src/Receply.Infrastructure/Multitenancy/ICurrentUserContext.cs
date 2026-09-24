namespace Receply.Infrastructure.Multitenancy;

/// <summary>
/// Ambient holder for the staff member driving the current request, set by TenantResolutionMiddleware
/// from the JWT's staff-id claim when present. Null for webhook/background-queue paths, which
/// correctly represents "system/AI-originated" for audit stamping (see AuditSaveChangesInterceptor).
/// </summary>
public interface ICurrentUserContext
{
    Guid? StaffId { get; }
    void SetStaff(Guid staffId);
}

public class CurrentUserContext : ICurrentUserContext
{
    public Guid? StaffId { get; private set; }

    public void SetStaff(Guid staffId) => StaffId = staffId;
}
