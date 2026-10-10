using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Application.Staffing.Commands.AddStaff;

public class AddStaffCommandHandler(IApplicationDbContext db) : IRequestHandler<AddStaffCommand, Guid>
{
    public async Task<Guid> Handle(AddStaffCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        StaffAccessGuard.EnsureCanManage(caller, request.BranchId);
        StaffAccessGuard.EnsureCanAssignRole(caller!, request.Role);

        var phoneTaken = await db.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (phoneTaken)
            throw new InvalidOperationException("A staff member with this phone number already exists.");

        var staff = Staff.Create(
            request.TenantId, request.FullName, request.Email, request.PhoneNumber, request.Role,
            request.BranchId, request.Designation, request.ExperienceYears);
        db.Staff.Add(staff);

        if (request.Services.Count > 0 && request.BranchId.HasValue)
        {
            // Auto-provision this staff member's own bookable calendar, seeded from the branch's
            // current working hours so they're immediately bookable without a separate setup step.
            var resource = Resource.Create(request.TenantId, request.BranchId.Value, request.FullName, ResourceType.Staff, staff.Id);

            var workingHours = await db.BranchWorkingHours
                .Where(h => h.BranchId == request.BranchId && !h.IsClosed)
                .ToListAsync(cancellationToken);

            foreach (var hours in workingHours.Where(h => h.OpenTime.HasValue && h.CloseTime.HasValue))
                resource.AddAvailabilityRule(hours.DayOfWeek, hours.OpenTime!.Value, hours.CloseTime!.Value);

            db.Resources.Add(resource);

            foreach (var service in request.Services)
                db.StaffServices.Add(StaffService.Create(request.TenantId, staff.Id, service.ServiceId, TimeSpan.FromMinutes(service.SessionDurationMinutes)));
        }

        await db.SaveChangesAsync(cancellationToken);
        return staff.Id;
    }
}
