using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Conversations.Queries.ListConversations;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/conversations")]
public class ConversationsController(ISender sender, ITenantContext tenantContext) : ControllerBase
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
}
