using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Conversations.Commands.ResumeAi;
using Receply.Application.Conversations.Commands.SendStaffReply;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Conversations.Queries.ListConversations;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController(ISender sender, ITenantContext tenantContext, ICurrentUserContext currentUserContext) : ControllerBase
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

        await sender.Send(new SendStaffReplyCommand(tenantId, staffId, id, request.Body), cancellationToken);
        return NoContent();
    }

    /// <summary>The "back to AI" toggle - hands the conversation back from a human to the AI.</summary>
    [HttpPost("{id:guid}/resume-ai")]
    public async Task<IActionResult> ResumeAi(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new ResumeAiCommand(tenantId, id), cancellationToken);
        return NoContent();
    }
}

public record SendReplyRequest(string Body);
