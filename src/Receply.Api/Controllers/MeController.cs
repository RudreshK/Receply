using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.UpdatePreferredLanguage;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

/// <summary>Self-service settings for the signed-in staff member.</summary>
[Authorize]
[ApiController]
[Route("api/me")]
public class MeController(ISender sender, ITenantContext tenantContext, ICurrentUserContext currentUserContext) : ControllerBase
{
    public record UpdatePreferredLanguageRequest(string? LanguageCode);

    [HttpPut("preferred-language")]
    public async Task<IActionResult> UpdatePreferredLanguage(UpdatePreferredLanguageRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId || currentUserContext.StaffId is not { } staffId)
            return Unauthorized();

        await sender.Send(new UpdatePreferredLanguageCommand(tenantId, staffId, request.LanguageCode), cancellationToken);
        return NoContent();
    }
}
