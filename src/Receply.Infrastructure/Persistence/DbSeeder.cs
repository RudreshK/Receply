using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Receply.Application.Channels;
using Receply.Domain.Channels;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Channels.WhatsApp;

namespace Receply.Infrastructure.Persistence;

/// <summary>
/// First-run bootstrap: creates a demo tenant bound to WhatsApp:DefaultPhoneNumberId so the
/// webhook has a ChannelAccount to resolve inbound messages against. Idempotent - safe to run on
/// every startup. Replace with a real tenant onboarding flow once one exists; this only exists so
/// a freshly connected WhatsApp number isn't a dead end.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        ReceplyDbContext db, IOptions<WhatsAppOptions> whatsAppOptions, IChannelCredentialProtector credentialProtector,
        ILogger logger, CancellationToken cancellationToken = default)
    {
        var phoneNumberId = whatsAppOptions.Value.DefaultPhoneNumberId;
        if (string.IsNullOrWhiteSpace(phoneNumberId))
            return;

        var existingAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.Type == ChannelType.WhatsApp && c.ExternalId == phoneNumberId, cancellationToken);

        if (existingAccount is not null)
        {
            // Backfills the one-time migration from "every account shares the platform token" to
            // "every account carries its own" - safe to run on every startup since it only touches
            // accounts that don't have their own credentials yet.
            if (existingAccount.AccessToken is null && !string.IsNullOrWhiteSpace(whatsAppOptions.Value.AccessToken))
            {
                existingAccount.SetCredentials(credentialProtector.Protect(whatsAppOptions.Value.AccessToken), wabaId: null);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Backfilled a per-account WhatsApp credential for channel account {ChannelAccountId}.", existingAccount.Id);
            }

            return;
        }

        var tenant = Tenant.Create("Demo Business", BusinessType.Other, "Asia/Kolkata");
        var branch = tenant.AddBranch("Main Branch", "");

        var consultation = Service.Create(tenant.Id, "Consultation", TimeSpan.FromMinutes(30), 0m);
        var generalAppointment = Service.Create(tenant.Id, "General Appointment", TimeSpan.FromMinutes(30), 0m);

        var resource = Resource.Create(tenant.Id, branch.Id, "Front Desk", ResourceType.Staff);
        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday })
            resource.AddAvailabilityRule(day, new TimeOnly(9, 0), new TimeOnly(18, 0));

        var channelAccount = ChannelAccount.Create(tenant.Id, ChannelType.WhatsApp, phoneNumberId, "Primary WhatsApp");

        // Gives this seeded account its own encrypted copy of the platform token, same as a real
        // tenant's WhatsApp connection will, instead of relying on WhatsAppCloudApiProvider's shared-
        // token fallback forever.
        if (!string.IsNullOrWhiteSpace(whatsAppOptions.Value.AccessToken))
            channelAccount.SetCredentials(credentialProtector.Protect(whatsAppOptions.Value.AccessToken), wabaId: null);

        // Fixed demo phone number so OTP login is testable immediately after a fresh seed.
        var owner = Staff.Create(tenant.Id, "Demo Owner", "owner@demo.receply.in", "+10000000000", StaffRole.Owner);

        db.Tenants.Add(tenant);
        db.Services.AddRange(consultation, generalAppointment);
        db.Resources.Add(resource);
        db.ChannelAccounts.Add(channelAccount);
        db.Staff.Add(owner);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded demo tenant {TenantId} ({TenantName}) with WhatsApp channel account for phone_number_id {PhoneNumberId} and demo staff login {PhoneNumber}.",
            tenant.Id, tenant.Name, phoneNumberId, owner.PhoneNumber);
    }
}
