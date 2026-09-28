using MediatR;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features.Commands.SetFeatureEnabled;

/// <summary>Turns one Feature on or off for one tenant - what a support/admin tool, or eventually a
/// billing webhook reacting to a plan change, calls.</summary>
public record SetFeatureEnabledCommand(Guid TenantId, Feature Feature, bool IsEnabled) : IRequest;
