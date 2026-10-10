using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Receply.Domain.Platform;

namespace Receply.Infrastructure.Persistence;

/// <summary>First-run bootstrap for the single PlatformAdmin account, mirroring DbSeeder's
/// idempotent "create once, then no-op" shape. Does nothing until PlatformAdmin:PhoneNumber is set.</summary>
public static class PlatformAdminSeeder
{
    public static async Task SeedAsync(
        ReceplyDbContext db, IOptions<PlatformAdminOptions> options, ILogger logger, CancellationToken cancellationToken = default)
    {
        var phoneNumber = options.Value.PhoneNumber;
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return;

        if (await db.PlatformAdmins.AnyAsync(a => a.PhoneNumber == phoneNumber, cancellationToken))
            return;

        var admin = PlatformAdmin.Create(options.Value.FullName, options.Value.Email, phoneNumber);
        db.PlatformAdmins.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded platform admin {PlatformAdminId} for login {PhoneNumber}.", admin.Id, admin.PhoneNumber);
    }
}
