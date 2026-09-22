using Microsoft.EntityFrameworkCore;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;
using Receply.Domain.Crm;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;

namespace Receply.Application.Common;

/// <summary>
/// Persistence seam the Application layer codes against, implemented by Receply.Infrastructure's
/// ReceplyDbContext. Keeps Application free of a direct EF/Infrastructure dependency.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Staff> Staff { get; }
    DbSet<Client> Clients { get; }
    DbSet<ChannelAccount> ChannelAccounts { get; }
    DbSet<Service> Services { get; }
    DbSet<Resource> Resources { get; }
    DbSet<AvailabilityRule> AvailabilityRules { get; }
    DbSet<TimeBlock> TimeBlocks { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<Message> Messages { get; }
    DbSet<LoginOtp> LoginOtps { get; }
    DbSet<BranchWorkingHours> BranchWorkingHours { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<Faq> Faqs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
