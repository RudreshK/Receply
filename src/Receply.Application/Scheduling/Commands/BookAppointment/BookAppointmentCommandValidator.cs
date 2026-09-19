using FluentValidation;

namespace Receply.Application.Scheduling.Commands.BookAppointment;

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.LocationId).NotEmpty();
        RuleFor(x => x.StartUtc).GreaterThan(DateTimeOffset.UtcNow).WithMessage("Appointment must start in the future.");
    }
}
