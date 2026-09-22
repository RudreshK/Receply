using MediatR;

namespace Receply.Application.Tenancy.Commands.UpdateTenantSettings;

public record UpdateTenantSettingsCommand(Guid TenantId, string Name, string BusinessType, string TimeZoneId) : IRequest;
