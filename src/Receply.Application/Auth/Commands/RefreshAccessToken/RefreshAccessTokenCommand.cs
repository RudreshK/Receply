using MediatR;
using Receply.Application.Auth.Commands.VerifyOtp;

namespace Receply.Application.Auth.Commands.RefreshAccessToken;

/// <summary>Exchanges a still-valid refresh token for a new access token, without asking the staff
/// member to sign in again. The refresh token itself is rotated (single-use).</summary>
public record RefreshAccessTokenCommand(string RefreshToken) : IRequest<VerifyOtpResult>;
