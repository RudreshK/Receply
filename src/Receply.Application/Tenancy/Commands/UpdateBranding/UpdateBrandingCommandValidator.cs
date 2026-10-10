using FluentValidation;

namespace Receply.Application.Tenancy.Commands.UpdateBranding;

public class UpdateBrandingCommandValidator : AbstractValidator<UpdateBrandingCommand>
{
    // A 2 MB image resized client-side and base64-encoded as a data: URL runs to roughly 2.7 MB of
    // text; this caps it generously above that without allowing arbitrarily large uploads.
    private const int MaxImageDataUrlLength = 2_900_000;

    public UpdateBrandingCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

        RuleFor(x => x.BusinessEmail).EmailAddress().MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.BusinessEmail));
        RuleFor(x => x.Website).MaximumLength(200);
        RuleFor(x => x.OwnerName).MaximumLength(60);
        RuleFor(x => x.Designation).MaximumLength(60);

        RuleFor(x => x.LogoDataUrl)
            .Must(url => url!.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Logo must be an uploaded image.")
            .MaximumLength(MaxImageDataUrlLength)
            .When(x => !string.IsNullOrEmpty(x.LogoDataUrl));

        RuleFor(x => x.OwnerPhotoDataUrl)
            .Must(url => url!.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Owner photo must be an uploaded image.")
            .MaximumLength(MaxImageDataUrlLength)
            .When(x => !string.IsNullOrEmpty(x.OwnerPhotoDataUrl));
    }
}
