using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Channels;
using Receply.Domain.Common;
using Receply.Domain.Conversations;
using Receply.Domain.Crm;
using Receply.Domain.Platform;
using Receply.Domain.Scheduling;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Infrastructure.Persistence;

public class ReceplyDbContext(DbContextOptions<ReceplyDbContext> options, ITenantContext tenantContext)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ChannelAccount> ChannelAccounts => Set<ChannelAccount>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<TimeBlock> TimeBlocks => Set<TimeBlock>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentFollowUp> AppointmentFollowUps => Set<AppointmentFollowUp>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<LoginOtp> LoginOtps => Set<LoginOtp>();
    public DbSet<SignupOtp> SignupOtps => Set<SignupOtp>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BranchWorkingHours> BranchWorkingHours => Set<BranchWorkingHours>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<MessageTranslation> MessageTranslations => Set<MessageTranslation>();
    public DbSet<TenantFeatureFlag> TenantFeatureFlags => Set<TenantFeatureFlag>();
    public DbSet<PlatformAdmin> PlatformAdmins => Set<PlatformAdmin>();
    public DbSet<PlatformAdminLoginOtp> PlatformAdminLoginOtps => Set<PlatformAdminLoginOtp>();
    public DbSet<BusinessEnquiry> BusinessEnquiries => Set<BusinessEnquiry>();

    /// <summary>Entities that opt out of optimistic concurrency tracking - immutable/high-volume rows where it adds no value.</summary>
    private static readonly Type[] RowVersionExemptTypes = [typeof(Message), typeof(MessageTranslation)];

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReceplyDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(Entity).IsAssignableFrom(entityType.ClrType))
                continue;

            var entityBuilder = modelBuilder.Entity(entityType.ClrType);

            // Every tenant-owned aggregate is scoped to the current request's tenant automatically,
            // so application code can't accidentally leak data across tenants by forgetting a Where().
            // Soft-deleted rows are excluded the same way, for the same reason.
            if (typeof(TenantOwnedEntity).IsAssignableFrom(entityType.ClrType))
                entityBuilder.HasQueryFilter(BuildFilter(nameof(BuildTenantAndSoftDeleteFilter), entityType.ClrType));
            else if (entityType.ClrType == typeof(Tenant))
                entityBuilder.HasQueryFilter(BuildFilter(nameof(BuildSoftDeleteFilter), entityType.ClrType));

            // Every Id is a client-generated Guid (see Entity.cs) - without this, EF Core can't tell
            // whether a non-default key on an entity discovered via graph traversal (e.g. appended to
            // an already-loaded collection navigation, rather than passed to DbSet.Add) means "new" or
            // "already persisted", and can misclassify a brand-new child as Modified instead of Added.
            entityBuilder.Property(nameof(Entity.Id)).ValueGeneratedNever();

            entityBuilder.Property(nameof(Entity.CreatedOn)).HasPrecision(0);

            // Message ignores ModifiedOn/ModifiedBy/RowVersion entirely (see MessageConfiguration) -
            // skip configuring what it doesn't map, and don't apply optimistic concurrency to a
            // write-once row where "was this modified concurrently" never applies.
            if (!RowVersionExemptTypes.Contains(entityType.ClrType))
            {
                entityBuilder.Property(nameof(Entity.ModifiedOn)).HasPrecision(0);
                entityBuilder.Property<uint>(nameof(Entity.RowVersion)).HasColumnName("xmin").IsRowVersion();
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private LambdaExpression BuildFilter(string methodName, Type entityClrType)
    {
        var method = typeof(ReceplyDbContext)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .MakeGenericMethod(entityClrType);

        return (LambdaExpression)method.Invoke(this, null)!;
    }

    private LambdaExpression BuildTenantAndSoftDeleteFilter<TEntity>() where TEntity : TenantOwnedEntity
    {
        Expression<Func<TEntity, bool>> filter =
            entity => (!tenantContext.TenantId.HasValue || entity.TenantId == tenantContext.TenantId) && !entity.IsDeleted;
        return filter;
    }

    private LambdaExpression BuildSoftDeleteFilter<TEntity>() where TEntity : Entity
    {
        Expression<Func<TEntity, bool>> filter = entity => !entity.IsDeleted;
        return filter;
    }
}
