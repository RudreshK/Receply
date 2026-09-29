using FluentValidation;

namespace Receply.Application.Scheduling.Commands.AddAppointmentFollowUp;

public class AddAppointmentFollowUpCommandValidator : AbstractValidator<AddAppointmentFollowUpCommand>
{
    private static readonly int[] AllowedOffsets = [1, 2, 3];

    public AddAppointmentFollowUpCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Instructions).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.DayOffsets)
            .NotEmpty().WithMessage("Pick at least one day to send the follow-up.")
            .Must(offsets => offsets.Distinct().Count() == offsets.Count)
            .WithMessage("Days can only be picked once each.")
            .Must(offsets => offsets.All(AllowedOffsets.Contains))
            .WithMessage("Follow-ups can only be scheduled 1, 2, or 3 days out.");
    }
}
