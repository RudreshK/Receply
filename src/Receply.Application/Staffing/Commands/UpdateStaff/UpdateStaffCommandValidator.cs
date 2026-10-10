using FluentValidation;

namespace Receply.Application.Staffing.Commands.UpdateStaff;

public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActingStaffId).NotEmpty();
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.ExperienceYears).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Designation).MaximumLength(200);
        RuleForEach(x => x.Services).ChildRules(service =>
        {
            service.RuleFor(s => s.ServiceId).NotEmpty();
            service.RuleFor(s => s.SessionDurationMinutes).GreaterThan(0);
        });
    }
}
