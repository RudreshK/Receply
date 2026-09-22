using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>A question/answer pair the AI receptionist can draw on for auto-replies.</summary>
public class Faq : TenantOwnedEntity
{
    public string Question { get; private set; } = default!;
    public string Answer { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private Faq() { }

    public static Faq Create(Guid tenantId, string question, string answer)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question is required.", nameof(question));
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Answer is required.", nameof(answer));

        var faq = new Faq
        {
            Question = question,
            Answer = answer
        };
        faq.TenantId = tenantId;
        return faq;
    }

    public void Update(string question, string answer)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new ArgumentException("Question is required.", nameof(question));
        if (string.IsNullOrWhiteSpace(answer))
            throw new ArgumentException("Answer is required.", nameof(answer));

        Question = question;
        Answer = answer;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
