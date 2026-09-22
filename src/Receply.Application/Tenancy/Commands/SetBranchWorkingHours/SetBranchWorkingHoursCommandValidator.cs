using FluentValidation;

namespace Receply.Application.Tenancy.Commands.SetBranchWorkingHours;

public class SetBranchWorkingHoursCommandValidator : AbstractValidator<SetBranchWorkingHoursCommand>
{
    public SetBranchWorkingHoursCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.OpenTime).NotNull().When(e => !e.IsClosed);
            entry.RuleFor(e => e.CloseTime).NotNull().When(e => !e.IsClosed);
            entry.RuleFor(e => e.CloseTime)
                .GreaterThan(e => e.OpenTime!.Value)
                .When(e => !e.IsClosed && e.OpenTime.HasValue && e.CloseTime.HasValue)
                .WithMessage("Close time must be after open time.");
        });
    }
}
