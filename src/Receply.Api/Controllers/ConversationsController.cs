using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Conversations.Commands.ResumeAi;
using Receply.Application.Conversations.Commands.SendStaffReply;
using Receply.Application.Conversations.Commands.TranslateMessage;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Conversations.Queries.ListConversations;
using Receply.Application.Features;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController(
    ISender sender,
    ITenantContext tenantContext,
    ICurrentUserContext currentUserContext,
    IFeatureAccess featureAccess) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ConversationListItemDto>>> List(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListConversationsQuery(tenantId), cancellationToken);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<ActionResult<List<ConversationMessageDto>>> GetMessages(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetConversationMessagesQuery(tenantId, id), cancellationToken);
    }

    /// <summary>A staff member replying by hand; this takes the conversation over from the AI.</summary>
    [HttpPost("{id:guid}/reply")]
    public async Task<IActionResult> Reply(Guid id, [FromBody] SendReplyRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId || currentUserContext.StaffId is not { } staffId)
            return Unauthorized();

        if (!await featureAccess.IsEnabledAsync(tenantId, Feature.ManualStaffReply, cancellationToken))
            return FeatureDisabled();

        await sender.Send(new SendStaffReplyCommand(tenantId, staffId, id, request.Body), cancellationToken);
        return NoContent();
    }

    /// <summary>The "back to AI" toggle - hands the conversation back from a human to the AI.</summary>
    [HttpPost("{id:guid}/resume-ai")]
    public async Task<IActionResult> ResumeAi(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        if (!await featureAccess.IsEnabledAsync(tenantId, Feature.ManualStaffReply, cancellationToken))
            return FeatureDisabled();

        await sender.Send(new ResumeAiCommand(tenantId, id), cancellationToken);
        return NoContent();
    }

    /// <summary>Translates one message into the requested language, for a staff member who doesn't read the customer's/AI's language.</summary>
    [HttpPost("{id:guid}/messages/{messageId:guid}/translate")]
    public async Task<ActionResult<TranslateMessageResult>> Translate(
        Guid id, Guid messageId, [FromBody] TranslateRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        if (!await featureAccess.IsEnabledAsync(tenantId, Feature.MessageTranslation, cancellationToken))
            return FeatureDisabled();

        return await sender.Send(new TranslateMessageCommand(tenantId, id, messageId, request.LanguageCode), cancellationToken);
    }

    private ObjectResult FeatureDisabled() =>
        StatusCode(StatusCodes.Status403Forbidden, new { message = "This feature isn't available on your plan." });
}

public record SendReplyRequest(string Body);
public record TranslateRequest(string LanguageCode);
