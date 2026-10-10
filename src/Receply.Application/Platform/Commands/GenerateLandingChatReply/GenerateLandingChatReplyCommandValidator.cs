using FluentValidation;

namespace Receply.Application.Platform.Commands.GenerateLandingChatReply;

public class GenerateLandingChatReplyCommandValidator : AbstractValidator<GenerateLandingChatReplyCommand>
{
    // Anonymous/unauthenticated endpoint - these caps keep one visitor from running an unbounded
    // (and unbounded-cost) conversation against the AI provider.
    public GenerateLandingChatReplyCommandValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.History).Must(h => h.Count <= 30).WithMessage("Conversation is too long.");
        RuleForEach(x => x.History).ChildRules(message =>
        {
            message.RuleFor(m => m.Role).Must(r => r is "user" or "assistant").WithMessage("Role must be 'user' or 'assistant'.");
            message.RuleFor(m => m.Text).NotEmpty().MaximumLength(1000);
        });
    }
}
