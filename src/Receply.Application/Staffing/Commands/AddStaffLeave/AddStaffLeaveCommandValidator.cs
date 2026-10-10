using FluentValidation;

namespace Receply.Application.Staffing.Commands.AddStaffLeave;

public class AddStaffLeaveCommandValidator : AbstractValidator<AddStaffLeaveCommand>
{
    public AddStaffLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.ActingStaffId).NotEmpty();
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(500);
        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime!.Value)
            .When(x => x.StartTime.HasValue && x.EndTime.HasValue)
            .WithMessage("End time must be after start time.");
        RuleFor(x => x)
            .Must(x => x.StartTime.HasValue == x.EndTime.HasValue)
            .WithMessage("Start and end time must both be set, or both left unset for a full-day leave.");
    }
}
