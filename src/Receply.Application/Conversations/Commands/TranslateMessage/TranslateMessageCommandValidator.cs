using FluentValidation;

namespace Receply.Application.Conversations.Commands.TranslateMessage;

public class TranslateMessageCommandValidator : AbstractValidator<TranslateMessageCommand>
{
    public TranslateMessageCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.MessageId).NotEmpty();
        RuleFor(x => x.LanguageCode).NotEmpty().MaximumLength(16);
    }
}
