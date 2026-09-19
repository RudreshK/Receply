using System.Globalization;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Application.Conversations.Commands.RequestHandoff;
using Receply.Application.Scheduling.Commands.BookAppointment;
using Receply.Application.Scheduling.Commands.CancelAppointment;
using Receply.Application.Scheduling.Commands.RescheduleAppointment;
using Receply.Application.Scheduling.Queries.GetAvailableSlots;
using Receply.Application.Scheduling.Queries.ListServices;
using Receply.Application.Scheduling.Queries.ListUpcomingAppointments;
using Receply.Domain.Scheduling;

namespace Receply.Application.AiEngine;

public class AiToolExecutor(ISender sender, IApplicationDbContext db) : IAiToolExecutor
{
    private static readonly JsonSerializerOptions ResultJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<(string ResultJson, bool IsError)> ExecuteAsync(
        string toolName, string argumentsJson, AiToolContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var args = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
            var root = args.RootElement;

            object result = toolName switch
            {
                AiToolCatalog.ListServices => await sender.Send(new ListServicesQuery(context.TenantId), cancellationToken),

                AiToolCatalog.CheckAvailability => await sender.Send(
                    new GetAvailableSlotsQuery(context.TenantId, GetString(root, "service_name"), DateOnly.Parse(GetString(root, "date"))),
                    cancellationToken),

                AiToolCatalog.BookAppointment => await BookAsync(root, context, cancellationToken),

                AiToolCatalog.ListMyAppointments => await sender.Send(
                    new ListUpcomingAppointmentsQuery(context.TenantId, context.ClientId), cancellationToken),

                AiToolCatalog.RescheduleAppointment => await RescheduleAsync(root, context, cancellationToken),

                AiToolCatalog.CancelAppointment => await CancelAsync(root, context, cancellationToken),

                AiToolCatalog.RequestHumanHandoff => await HandoffAsync(root, context, cancellationToken),

                _ => throw new InvalidOperationException($"Unknown tool '{toolName}'.")
            };

            return (JsonSerializer.Serialize(result, ResultJsonOptions), false);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or ValidationException or FormatException)
        {
            return (JsonSerializer.Serialize(new { error = ex.Message }, ResultJsonOptions), true);
        }
    }

    private async Task<object> BookAsync(JsonElement args, AiToolContext context, CancellationToken cancellationToken)
    {
        var serviceName = GetString(args, "service_name");
        var startUtc = ParseDateTimeOffset(GetString(args, "start_time"));

        var service = await db.Services.FirstOrDefaultAsync(
            s => s.TenantId == context.TenantId && s.IsActive && s.Name.ToLower() == serviceName.ToLower(), cancellationToken)
            ?? throw new KeyNotFoundException($"No active service named '{serviceName}'.");

        var candidateResources = TryGetGuid(args, "resource_id", out var explicitResourceId)
            ? await db.Resources.Where(r => r.Id == explicitResourceId && r.TenantId == context.TenantId && r.IsActive).ToListAsync(cancellationToken)
            : await db.Resources.Where(r => r.TenantId == context.TenantId && r.IsActive).OrderBy(r => r.Id).ToListAsync(cancellationToken);

        if (candidateResources.Count == 0)
            throw new KeyNotFoundException("No matching resource is available to book this service.");

        foreach (var resource in candidateResources)
        {
            try
            {
                var appointmentId = await sender.Send(
                    new BookAppointmentCommand(context.TenantId, context.ClientId, service.Id, resource.Id, resource.BranchId, startUtc),
                    cancellationToken);

                return new
                {
                    appointment_id = appointmentId,
                    service_name = service.Name,
                    resource_name = resource.Name,
                    start_time = startUtc,
                    end_time = startUtc + service.Duration
                };
            }
            catch (InvalidOperationException)
            {
                // That resource is already booked at this time - try the next candidate.
            }
        }

        throw new InvalidOperationException("That time is no longer available. Call check_availability again for other options.");
    }

    private async Task<object> RescheduleAsync(JsonElement args, AiToolContext context, CancellationToken cancellationToken)
    {
        var appointmentId = GetGuid(args, "appointment_id");
        var newStartUtc = ParseDateTimeOffset(GetString(args, "new_start_time"));

        await EnsureOwnedAppointment(appointmentId, context, cancellationToken);

        await sender.Send(new RescheduleAppointmentCommand(context.TenantId, appointmentId, newStartUtc), cancellationToken);

        return new { appointment_id = appointmentId, new_start_time = newStartUtc, status = "rescheduled" };
    }

    private async Task<object> CancelAsync(JsonElement args, AiToolContext context, CancellationToken cancellationToken)
    {
        var appointmentId = GetGuid(args, "appointment_id");
        var reason = TryGetString(args, "reason", out var r) ? r : null;

        await EnsureOwnedAppointment(appointmentId, context, cancellationToken);

        await sender.Send(new CancelAppointmentCommand(context.TenantId, appointmentId, reason), cancellationToken);

        return new { appointment_id = appointmentId, status = "cancelled" };
    }

    private async Task<object> HandoffAsync(JsonElement args, AiToolContext context, CancellationToken cancellationToken)
    {
        var reason = GetString(args, "reason");
        await sender.Send(new RequestHandoffCommand(context.ConversationId, reason), cancellationToken);
        return new { status = "handoff_requested" };
    }

    private async Task EnsureOwnedAppointment(Guid appointmentId, AiToolContext context, CancellationToken cancellationToken)
    {
        var owned = await db.Appointments.AnyAsync(
            a => a.Id == appointmentId && a.TenantId == context.TenantId && a.ClientId == context.ClientId, cancellationToken);

        if (!owned)
            throw new KeyNotFoundException("No appointment with that id was found for this customer.");
    }

    private static string GetString(JsonElement element, string propertyName) =>
        TryGetString(element, propertyName, out var value) ? value : throw new InvalidOperationException($"Missing required argument '{propertyName}'.");

    private static bool TryGetString(JsonElement element, string propertyName, out string value)
    {
        if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
        {
            value = prop.GetString()!;
            return true;
        }
        value = string.Empty;
        return false;
    }

    private static Guid GetGuid(JsonElement element, string propertyName) =>
        TryGetGuid(element, propertyName, out var value) ? value : throw new InvalidOperationException($"Missing required argument '{propertyName}'.");

    private static bool TryGetGuid(JsonElement element, string propertyName, out Guid value)
    {
        if (TryGetString(element, propertyName, out var raw) && Guid.TryParse(raw, out value))
            return true;
        value = Guid.Empty;
        return false;
    }

    private static DateTimeOffset ParseDateTimeOffset(string raw) =>
        DateTimeOffset.Parse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
