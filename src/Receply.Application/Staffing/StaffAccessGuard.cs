using Receply.Domain.Tenancy;

namespace Receply.Application.Staffing;

/// <summary>
/// Who may add/edit/activate/deactivate staff (and manage their leave): the tenant-wide Owner for
/// any branch, or a BranchOwner for their own branch only. Every other role has no staff-management
/// rights at all.
/// </summary>
public static class StaffAccessGuard
{
    public static void EnsureCanManage(Staff? caller, Guid? targetBranchId)
    {
        if (caller is null || !caller.IsActive)
            throw new UnauthorizedAccessException("You don't have permission to manage staff.");

        if (caller.Role == StaffRole.Owner)
            return;

        if (caller.Role == StaffRole.BranchOwner && targetBranchId.HasValue && caller.BranchId == targetBranchId)
            return;

        throw new UnauthorizedAccessException("You don't have permission to manage staff for this branch.");
    }

    /// <summary>Only the tenant Owner can grant Owner or BranchOwner rights - a BranchOwner can't promote anyone.</summary>
    public static void EnsureCanAssignRole(Staff caller, StaffRole role)
    {
        if ((role == StaffRole.Owner || role == StaffRole.BranchOwner) && caller.Role != StaffRole.Owner)
            throw new UnauthorizedAccessException("Only the business owner can assign that role.");
    }
}
