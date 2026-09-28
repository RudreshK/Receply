using MediatR;

namespace Receply.Application.Tenancy.Commands.UpdatePreferredLanguage;

/// <summary>A staff member setting their own display language for the dashboard's Translate feature.
/// Null clears the preference (dashboard falls back to English).</summary>
public record UpdatePreferredLanguageCommand(Guid TenantId, Guid StaffId, string? LanguageCode) : IRequest;
