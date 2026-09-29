using FluentValidation;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Commands.UpdateAppointmentStatus;

public class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();

        // Only the two post-hoc "how did it actually go" outcomes are settable through this command -
        // booking, rescheduling, and cancelling all have their own dedicated commands already.
        RuleFor(x => x.NewStatus).Must(s => s is AppointmentStatus.Completed or AppointmentStatus.NoShow)
            .WithMessage("Status can only be changed to Completed or NoShow.");
    }
}
