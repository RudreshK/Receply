using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Auth.Commands.CompleteSignup;
using Receply.Application.Auth.Commands.RequestSignupOtp;
using Receply.Application.Auth.Commands.VerifyOtp;

namespace Receply.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/signup")]
public class SignupController(ISender sender, IHostEnvironment environment) : ControllerBase
{
    public record RequestSignupOtpRequest(string PhoneNumber);
    public record RequestSignupOtpResponse(string? Code);

    public record CompleteSignupRequest(
        string PhoneNumber,
        string Code,
        string BusinessName,
        string BusinessType,
        string TimeZoneId,
        string OwnerFirstName,
        string OwnerLastName,
        string OwnerEmail);

    [HttpPost("request-otp")]
    public async Task<ActionResult<RequestSignupOtpResponse>> RequestOtp(RequestSignupOtpRequest request, CancellationToken cancellationToken)
    {
        var code = await sender.Send(new RequestSignupOtpCommand(request.PhoneNumber), cancellationToken);

        // The code is only ever echoed back in Development, where no real OTP gateway is wired up -
        // in every other environment it's delivered exclusively via IOtpSender.
        return new RequestSignupOtpResponse(environment.IsDevelopment() ? code : null);
    }

    [HttpPost("complete")]
    public async Task<ActionResult<VerifyOtpResult>> Complete(CompleteSignupRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CompleteSignupCommand(
                request.PhoneNumber, request.Code, request.BusinessName, request.BusinessType,
                request.TimeZoneId, request.OwnerFirstName, request.OwnerLastName, request.OwnerEmail),
            cancellationToken);

        return Ok(result);
    }
}
