using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Channels;
using Receply.Domain.Common;
using Receply.Domain.Conversations;
using Receply.Domain.Crm;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Infrastructure.Persistence;

public class ReceplyDbContext(DbContextOptions<ReceplyDbContext> options, ITenantContext tenantContext)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ChannelAccount> ChannelAccounts => Set<ChannelAccount>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<TimeBlock> TimeBlocks => Set<TimeBlock>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceplyDbContext).Assembly);

        // Every tenant-owned aggregate is scoped to the current request's tenant automatically,
        // so application code can't accidentally leak data across tenants by forgetting a Where().
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(TenantOwnedEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(ReceplyDbContext)
                .GetMethod(nameof(BuildTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            var filter = method.Invoke(this, null);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter((System.Linq.Expressions.LambdaExpression)filter!);
        }

        base.OnModelCreating(modelBuilder);
    }

    private System.Linq.Expressions.LambdaExpression BuildTenantFilter<TEntity>() where TEntity : TenantOwnedEntity
    {
        System.Linq.Expressions.Expression<Func<TEntity, bool>> filter =
            entity => !tenantContext.TenantId.HasValue || entity.TenantId == tenantContext.TenantId;
        return filter;
    }
}
