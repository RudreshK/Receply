using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Application.Staffing.Commands.UpdateStaff;

public class UpdateStaffCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateStaffCommand>
{
    public async Task Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        var target = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        StaffAccessGuard.EnsureCanManage(caller, target.BranchId);

        target.UpdateProfile(request.FullName, request.Email, request.Designation, request.ExperienceYears);

        var existingAssignments = await db.StaffServices.Where(ss => ss.StaffId == target.Id).ToListAsync(cancellationToken);
        foreach (var assignment in existingAssignments)
            db.StaffServices.Remove(assignment);

        foreach (var service in request.Services)
            db.StaffServices.Add(StaffService.Create(request.TenantId, target.Id, service.ServiceId, TimeSpan.FromMinutes(service.SessionDurationMinutes)));

        var resource = await db.Resources.FirstOrDefaultAsync(r => r.StaffId == target.Id, cancellationToken);
        if (resource is null && request.Services.Count > 0 && target.BranchId.HasValue)
        {
            resource = Resource.Create(request.TenantId, target.BranchId.Value, target.FullName, ResourceType.Staff, target.Id);

            var workingHours = await db.BranchWorkingHours
                .Where(h => h.BranchId == target.BranchId && !h.IsClosed)
                .ToListAsync(cancellationToken);

            foreach (var hours in workingHours.Where(h => h.OpenTime.HasValue && h.CloseTime.HasValue))
                resource.AddAvailabilityRule(hours.DayOfWeek, hours.OpenTime!.Value, hours.CloseTime!.Value);

            db.Resources.Add(resource);
        }
        else if (resource is not null)
        {
            // Keep the auto-provisioned resource's active state in sync with whether this staff
            // member currently performs any bookable service.
            if (request.Services.Count == 0)
                resource.Deactivate();
            else
                resource.Activate();
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
