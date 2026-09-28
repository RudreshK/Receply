namespace Receply.Domain.Tenancy;

/// <summary>
/// A capability that can be turned on or off per tenant, independent of the code that implements it -
/// the mechanism behind subscription plans/trials/add-ons. Add a new case here whenever you build
/// something a business might or might not have paid for, and gate its handlers/endpoints on it via
/// IFeatureAccess instead of shipping it unconditionally on for every tenant.
/// </summary>
public enum Feature
{
    /// <summary>Staff can reply to a customer by hand from the dashboard, and hand a conversation back to the AI.</summary>
    ManualStaffReply,

    /// <summary>WhatsApp read receipts (blue ticks) and the "typing…" indicator shown to customers.</summary>
    ReadReceipts,

    /// <summary>On-demand translation of a message into a staff member's preferred language.</summary>
    MessageTranslation,

    /// <summary>Automated WhatsApp reminders sent ahead of a customer's upcoming appointment.</summary>
    AppointmentReminders
}
