using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Auth.Commands.RequestOtp;
using Receply.Application.Auth.Commands.VerifyOtp;

namespace Receply.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender, IHostEnvironment environment) : ControllerBase
{
    public record RequestOtpRequest(string PhoneNumber);
    public record RequestOtpResponse(string? Code);
    public record VerifyOtpRequest(string PhoneNumber, string Code);

    [HttpPost("request-otp")]
    public async Task<ActionResult<RequestOtpResponse>> RequestOtp(RequestOtpRequest request, CancellationToken cancellationToken)
    {
        var code = await sender.Send(new RequestOtpCommand(request.PhoneNumber), cancellationToken);

        // The code is only ever echoed back in Development, where no real OTP gateway is wired up -
        // in every other environment it's delivered exclusively via IOtpSender.
        return new RequestOtpResponse(environment.IsDevelopment() ? code : null);
    }

    [HttpPost("verify-otp")]
    public async Task<ActionResult<VerifyOtpResult>> VerifyOtp(VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new VerifyOtpCommand(request.PhoneNumber, request.Code), cancellationToken);
        return Ok(result);
    }
}
