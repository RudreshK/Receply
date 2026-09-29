using MediatR;

namespace Receply.Application.Auth.Commands.RevokeRefreshToken;

/// <summary>An explicit sign-out - invalidates the refresh token server-side so it can't be replayed
/// after the staff member has logged out, even if it was captured.</summary>
public record RevokeRefreshTokenCommand(string RefreshToken) : IRequest;
