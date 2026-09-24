using Receply.Application.AiEngine.Contracts;

namespace Receply.Application.AiEngine;

/// <summary>The fixed set of actions the AI receptionist can take. Names here are the contract with AiToolExecutor.</summary>
public static class AiToolCatalog
{
    public const string ListServices = "list_services";
    public const string CheckAvailability = "check_availability";
    public const string BookAppointment = "book_appointment";
    public const string ListMyAppointments = "list_my_appointments";
    public const string RescheduleAppointment = "reschedule_appointment";
    public const string CancelAppointment = "cancel_appointment";
    public const string RequestHumanHandoff = "request_human_handoff";

    public static IReadOnlyList<AiTool> All { get; } =
    [
        new AiTool(
            ListServices,
            "List the services this business offers, with duration and price. Call this if you're not sure of the exact service name or price to quote.",
            """{"type":"object","properties":{},"required":[]}"""),

        new AiTool(
            CheckAvailability,
            "Find available appointment slots for a given service on a given date. Always call this before booking, to get a real resource_id and start_time - never invent times.",
            """
            {
              "type": "object",
              "properties": {
                "service_name": { "type": "string", "description": "The service name, as returned by list_services." },
                "date": { "type": "string", "description": "Date to check, in YYYY-MM-DD format, in the business's local time zone." }
              },
              "required": ["service_name", "date"]
            }
            """),

        new AiTool(
            BookAppointment,
            "Book an appointment for the current customer. Use a resource_id and start_time you got from check_availability - don't guess.",
            """
            {
              "type": "object",
              "properties": {
                "service_name": { "type": "string", "description": "The service name, as returned by list_services." },
                "resource_id": { "type": "string", "description": "The resource id from check_availability for the chosen slot. Omit only if check_availability wasn't used." },
                "start_time": { "type": "string", "description": "ISO 8601 date-time for the appointment start, as returned by check_availability." }
              },
              "required": ["service_name", "start_time"]
            }
            """),

        new AiTool(
            ListMyAppointments,
            "List the current customer's upcoming appointments. Call this before rescheduling or cancelling so you can reference the right one instead of asking the customer for an ID.",
            """{"type":"object","properties":{},"required":[]}"""),

        new AiTool(
            RescheduleAppointment,
            "Move an existing appointment to a new time. Get appointment_id from list_my_appointments and the new slot from check_availability.",
            """
            {
              "type": "object",
              "properties": {
                "appointment_id": { "type": "string", "description": "The appointment id from list_my_appointments." },
                "new_start_time": { "type": "string", "description": "ISO 8601 date-time for the new start, as returned by check_availability." }
              },
              "required": ["appointment_id", "new_start_time"]
            }
            """),

        new AiTool(
            CancelAppointment,
            "Cancel an existing appointment. Get appointment_id from list_my_appointments.",
            """
            {
              "type": "object",
              "properties": {
                "appointment_id": { "type": "string", "description": "The appointment id from list_my_appointments." },
                "reason": { "type": "string", "description": "Why the customer is cancelling, if they said." }
              },
              "required": ["appointment_id"]
            }
            """),

        new AiTool(
            RequestHumanHandoff,
            "Hand the conversation off to a human teammate. Call this if the customer explicitly asks for a person, or if you can't resolve their request after a couple of attempts.",
            """
            {
              "type": "object",
              "properties": {
                "reason": { "type": "string", "description": "A short summary of why you're handing off, for the teammate picking this up." }
              },
              "required": ["reason"]
            }
            """)
    ];
}
