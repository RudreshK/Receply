using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Auth.Commands.RevokeRefreshToken;

public class RevokeRefreshTokenCommandHandler(IApplicationDbContext db) : IRequestHandler<RevokeRefreshTokenCommand>
{
    public async Task Handle(RevokeRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(request.RefreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        // Already gone/expired/unknown - logout is idempotent either way, and we don't leak which.
        if (stored is null)
            return;

        stored.Revoke();
        await db.SaveChangesAsync(cancellationToken);
    }
}
