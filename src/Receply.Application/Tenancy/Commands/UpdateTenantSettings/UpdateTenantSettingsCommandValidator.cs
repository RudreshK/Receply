using FluentValidation;

namespace Receply.Application.Tenancy.Commands.UpdateTenantSettings;

public class UpdateTenantSettingsCommandValidator : AbstractValidator<UpdateTenantSettingsCommand>
{
    public UpdateTenantSettingsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessType).NotEmpty();
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
    }
}
