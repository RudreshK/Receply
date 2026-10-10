using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Platform.Commands.GenerateLandingChatReply;

namespace Receply.Api.Controllers;

/// <summary>Backs the chat bubble on the public marketing site - anonymous, no tenant. See
/// GenerateLandingChatReplyCommandHandler for the sales-assistant system prompt and enquiry capture.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/landing-chat")]
public class LandingChatController(ISender sender) : ControllerBase
{
    public record ChatMessageRequest(string Role, string Text);
    public record SendMessageRequest(List<ChatMessageRequest> History, string Message);

    [HttpPost("message")]
    public async Task<ActionResult<LandingChatReplyDto>> SendMessage(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var history = request.History.Select(m => new LandingChatMessageDto(m.Role, m.Text)).ToList();
        return await sender.Send(new GenerateLandingChatReplyCommand(history, request.Message), cancellationToken);
    }
}
