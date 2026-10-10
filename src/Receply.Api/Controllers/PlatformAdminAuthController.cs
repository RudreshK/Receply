using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Platform.Auth.Commands.RequestPlatformAdminOtp;
using Receply.Application.Platform.Auth.Commands.VerifyPlatformAdminOtp;

namespace Receply.Api.Controllers;

/// <summary>Login for the platform admin console - entirely separate from staff/business auth
/// (see AuthController). Deliberately has no signup/self-serve path.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/platform-admin/auth")]
public class PlatformAdminAuthController(ISender sender, IHostEnvironment environment) : ControllerBase
{
    public record RequestOtpRequest(string PhoneNumber);
    public record RequestOtpResponse(string? Code);
    public record VerifyOtpRequest(string PhoneNumber, string Code);

    [HttpPost("request-otp")]
    public async Task<ActionResult<RequestOtpResponse>> RequestOtp(RequestOtpRequest request, CancellationToken cancellationToken)
    {
        var code = await sender.Send(new RequestPlatformAdminOtpCommand(request.PhoneNumber), cancellationToken);
        return new RequestOtpResponse(environment.IsDevelopment() ? code : null);
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<PlatformAdminLoginResult>> VerifyOtp(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyPlatformAdminOtpCommand(request.PhoneNumber, request.Code), cancellationToken);
        return Ok(result);
    }
}
