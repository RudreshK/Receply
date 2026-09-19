using FluentValidation;

namespace Receply.Application.Scheduling.Commands.RescheduleAppointment;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.NewStartUtc).GreaterThan(DateTimeOffset.UtcNow).WithMessage("New start time must be in the future.");
    }
}
