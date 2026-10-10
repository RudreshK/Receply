namespace Receply.Infrastructure.Persistence;

/// <summary>Bootstraps the one PlatformAdmin account (the product owner) from config/App Settings
/// rather than a hardcoded value, since a phone number shouldn't live in source control. Empty
/// PhoneNumber means "nothing to seed" - same no-op convention as WhatsAppOptions.DefaultPhoneNumberId.</summary>
public class PlatformAdminOptions
{
    public const string SectionName = "PlatformAdmin";

    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "Receply Admin";
}
