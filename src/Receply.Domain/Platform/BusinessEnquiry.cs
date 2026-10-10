using Receply.Domain.Common;
using Receply.Domain.Tenancy;

namespace Receply.Domain.Platform;

public enum EnquiryStatus
{
    New,
    Converted,
    Dismissed
}

/// <summary>A lead captured outside self-serve signup (a phone call, a walk-in, a referral) that the
/// platform admin logs by hand and later either converts into a real Tenant + Owner, or dismisses.</summary>
public class BusinessEnquiry : AggregateRoot
{
    public string BusinessName { get; private set; } = default!;
    public BusinessType BusinessType { get; private set; }
    public string ContactName { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Notes { get; private set; }
    public EnquiryStatus Status { get; private set; } = EnquiryStatus.New;
    public Guid? ConvertedTenantId { get; private set; }

    private BusinessEnquiry() { }

    public static BusinessEnquiry Create(
        string businessName, BusinessType businessType, string contactName, string phoneNumber, string? email, string? notes)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new ArgumentException("Business name is required.", nameof(businessName));
        if (string.IsNullOrWhiteSpace(contactName))
            throw new ArgumentException("Contact name is required.", nameof(contactName));
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        return new BusinessEnquiry
        {
            BusinessName = businessName,
            BusinessType = businessType,
            ContactName = contactName,
            PhoneNumber = phoneNumber,
            Email = email,
            Notes = notes
        };
    }

    public void MarkConverted(Guid tenantId)
    {
        if (Status != EnquiryStatus.New)
            throw new InvalidOperationException("This enquiry has already been resolved.");

        Status = EnquiryStatus.Converted;
        ConvertedTenantId = tenantId;
    }

    public void Dismiss()
    {
        if (Status != EnquiryStatus.New)
            throw new InvalidOperationException("This enquiry has already been resolved.");

        Status = EnquiryStatus.Dismissed;
    }
}
