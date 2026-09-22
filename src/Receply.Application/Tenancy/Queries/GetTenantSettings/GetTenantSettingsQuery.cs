using MediatR;

namespace Receply.Application.Tenancy.Queries.GetTenantSettings;

public record TenantSettingsDto(Guid TenantId, string Name, string BusinessType, string TimeZoneId, string Status);

public record GetTenantSettingsQuery(Guid TenantId) : IRequest<TenantSettingsDto>;
