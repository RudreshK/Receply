using Receply.Application.AiEngine.Contracts;

namespace Receply.Application.AiEngine;

/// <summary>
/// Tools available in a staff-originated conversation (see WhatsAppWebhookController) - deliberately
/// separate from AiToolCatalog so a customer conversation can never reach these by accident.
/// </summary>
public static class StaffAiToolCatalog
{
    public const string UpdateMyDetails = "update_my_details";
    public const string AddHoliday = "add_holiday";
    public const string RemoveHoliday = "remove_holiday";
    public const string ListHolidays = "list_holidays";

    public static IReadOnlyList<AiTool> All { get; } =
    [
        new AiTool(
            UpdateMyDetails,
            "Update the calling staff member's own name, email, or phone number. Only include the fields that are actually changing.",
            """
            {
              "type": "object",
              "properties": {
                "full_name": { "type": "string", "description": "New full name, if changing." },
                "email": { "type": "string", "description": "New email address, if changing." },
                "phone_number": { "type": "string", "description": "New phone number, if changing." }
              },
              "required": []
            }
            """),

        new AiTool(
            AddHoliday,
            "Add a day the business is closed.",
            """
            {
              "type": "object",
              "properties": {
                "date": { "type": "string", "description": "Date of the holiday, in YYYY-MM-DD format." },
                "name": { "type": "string", "description": "Short name for the holiday, e.g. 'Diwali' or 'Staff outing'." }
              },
              "required": ["date", "name"]
            }
            """),

        new AiTool(
            RemoveHoliday,
            "Remove a previously added holiday. Call list_holidays first to get its id.",
            """{"type":"object","properties":{"holiday_id":{"type":"string","description":"The holiday id from list_holidays."}},"required":["holiday_id"]}"""),

        new AiTool(
            ListHolidays,
            "List holidays in a date range.",
            """
            {
              "type": "object",
              "properties": {
                "from_date": { "type": "string", "description": "Start date, YYYY-MM-DD." },
                "to_date": { "type": "string", "description": "End date, YYYY-MM-DD." }
              },
              "required": ["from_date", "to_date"]
            }
            """)
    ];
}
