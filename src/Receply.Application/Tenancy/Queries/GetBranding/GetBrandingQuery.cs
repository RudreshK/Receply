using MediatR;

namespace Receply.Application.Tenancy.Queries.GetBranding;

public record BrandingDto(
    Guid TenantId,
    string Name,
    string? BusinessEmail,
    string? Website,
    string? LogoDataUrl,
    string? OwnerName,
    string? Designation,
    string? OwnerPhotoDataUrl);

public record GetBrandingQuery(Guid TenantId) : IRequest<BrandingDto>;
