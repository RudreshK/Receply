using FluentValidation;

namespace Receply.Application.Conversations.Commands.ResumeAi;

public class ResumeAiCommandValidator : AbstractValidator<ResumeAiCommand>
{
    public ResumeAiCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
    }
}
