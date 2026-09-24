using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.CreateFaq;
using Receply.Application.Tenancy.Commands.DeleteFaq;
using Receply.Application.Tenancy.Commands.UpdateFaq;
using Receply.Application.Tenancy.Queries.ListFaqs;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs")]
public class FaqsController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record CreateFaqRequest(string Question, string Answer);
    public record UpdateFaqRequest(string Question, string Answer, bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<FaqDto>>> List(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListFaqsQuery(tenantId), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFaqRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new CreateFaqCommand(tenantId, request.Question, request.Answer), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateFaqRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new UpdateFaqCommand(tenantId, id, request.Question, request.Answer, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new DeleteFaqCommand(tenantId, id), cancellationToken);
        return NoContent();
    }
}
