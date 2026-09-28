using FluentValidation;

namespace Receply.Application.Conversations.Commands.SendStaffReply;

public class SendStaffReplyCommandValidator : AbstractValidator<SendStaffReplyCommand>
{
    public SendStaffReplyCommandValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.StaffId).NotEmpty();
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
