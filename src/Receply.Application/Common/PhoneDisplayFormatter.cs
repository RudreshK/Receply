namespace Receply.Application.Common;

/// <summary>
/// WhatsApp/the DB store phone numbers without a leading "+" (that's the wire format the Cloud API
/// itself expects for outbound sends) - this formats a copy for display only, never touching the
/// stored value or anything sent back to WhatsApp.
/// </summary>
public static class PhoneDisplayFormatter
{
    public static string Format(string phoneNumber) =>
        phoneNumber.StartsWith('+') ? phoneNumber : $"+{phoneNumber}";
}
