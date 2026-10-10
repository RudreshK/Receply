using MediatR;

namespace Receply.Application.Tenancy.Commands.UpdateBranding;

public record UpdateBrandingCommand(
    Guid TenantId,
    string Name,
    string? BusinessEmail,
    string? Website,
    string? LogoDataUrl,
    string? OwnerName,
    string? Designation,
    string? OwnerPhotoDataUrl) : IRequest;
