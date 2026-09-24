using FluentValidation;

namespace Receply.Application.Conversations.Commands.RequestHandoff;

public class RequestHandoffCommandValidator : AbstractValidator<RequestHandoffCommand>
{
    public RequestHandoffCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
