using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

public enum TenantStatus
{
    TrialActive,
    Active,
    PastDue,
    Suspended
}

/// <summary>A subscription tier. What each one unlocks is defined by SubscriptionPlans in the
/// Application layer, not here - the domain just knows which plan a tenant is on.
/// Explicit values: Starter and AssistedSetup keep the ordinals the now-renamed Basic/Premium
/// already shipped with, so any tenant already on one of those isn't silently remapped - Growth
/// was added later and gets a fresh value instead of being inserted into the existing sequence.</summary>
public enum SubscriptionPlan
{
    Trial = 0,
    Starter = 1,
    Growth = 3,
    AssistedSetup = 2
}

public class Tenant : AggregateRoot
{
    private static readonly TimeSpan TrialDuration = TimeSpan.FromDays(14);

    public string Name { get; private set; } = default!;
    public BusinessType BusinessType { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public TenantStatus Status { get; private set; } = TenantStatus.TrialActive;

    public SubscriptionPlan Plan { get; private set; } = SubscriptionPlan.Trial;

    /// <summary>When the free trial runs out. Only meaningful while Plan is Trial.</summary>
    public DateTimeOffset? TrialEndsUtc { get; private set; }

    /// <summary>When a paid plan needs renewing. Null means it doesn't expire on its own - today
    /// that's every paid plan, since there's no payment gateway yet to collect a renewal; it's set
    /// once billing actually drives this.</summary>
    public DateTimeOffset? PlanRenewsUtc { get; private set; }

    /// <summary>Branding shown in place of "Receply" once a business sets its own - on the dashboard
    /// sidebar and header today. All optional: a tenant that hasn't set these still sees the default
    /// Receply brand.</summary>
    public string? BusinessEmail { get; private set; }
    public string? Website { get; private set; }
    public string? LogoDataUrl { get; private set; }
    public string? OwnerName { get; private set; }
    public string? Designation { get; private set; }
    public string? OwnerPhotoDataUrl { get; private set; }

    private readonly List<Branch> _branches = [];
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    private Tenant() { }

    public static Tenant Create(string name, BusinessType businessType, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        return new Tenant
        {
            Name = name,
            BusinessType = businessType,
            TimeZoneId = timeZoneId,
            TrialEndsUtc = DateTimeOffset.UtcNow.Add(TrialDuration)
        };
    }

    public Branch AddBranch(string name, string address)
    {
        var branch = Branch.Create(Id, name, address);
        _branches.Add(branch);
        return branch;
    }

    public void UpdateProfile(string name, BusinessType businessType, string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        Name = name;
        BusinessType = businessType;
        TimeZoneId = timeZoneId;
    }

    public void UpdateBranding(
        string name,
        string? businessEmail,
        string? website,
        string? logoDataUrl,
        string? ownerName,
        string? designation,
        string? ownerPhotoDataUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        Name = name;
        BusinessEmail = businessEmail;
        Website = website;
        LogoDataUrl = logoDataUrl;
        OwnerName = ownerName;
        Designation = designation;
        OwnerPhotoDataUrl = ownerPhotoDataUrl;
    }

    public void Suspend() => Status = TenantStatus.Suspended;

    public void Reactivate() => Status = TenantStatus.Active;

    /// <summary>Moves the tenant onto a different plan. There's no payment gateway behind this yet -
    /// for now this is how a self-serve pick (Settings) or a manual back-office change after an
    /// offline payment gets recorded. renewsUtc is null for "doesn't expire" (the common case today).</summary>
    public void ChangePlan(SubscriptionPlan plan, DateTimeOffset? renewsUtc)
    {
        Plan = plan;
        PlanRenewsUtc = renewsUtc;

        if (plan != SubscriptionPlan.Trial && Status is TenantStatus.TrialActive or TenantStatus.PastDue)
            Status = TenantStatus.Active;
    }

    /// <summary>Whether the tenant should currently have access, independent of what Status says - a
    /// trial that's run past its end date is "out of access" the moment that date passes, without
    /// needing a background sweep to flip the stored Status first.</summary>
    public bool HasActiveAccess(DateTimeOffset nowUtc) => Status switch
    {
        TenantStatus.Suspended => false,
        TenantStatus.PastDue => false,
        TenantStatus.TrialActive => TrialEndsUtc is null || nowUtc <= TrialEndsUtc,
        TenantStatus.Active => PlanRenewsUtc is null || nowUtc <= PlanRenewsUtc,
        _ => false
    };
}
